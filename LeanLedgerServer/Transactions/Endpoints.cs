namespace LeanLedgerServer.Transactions;

using System.Diagnostics;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Results;
using static TransactionFunctions;

public static class Endpoints {
    public static void MapTransactions(this IEndpointRouteBuilder endpoints) {
        var transactions = endpoints.MapGroup("transactions");
        transactions.MapGet("{id:guid}", GetTransaction);
        transactions.MapGet("", ListTransactions);
        transactions.MapPost("", CreateTransaction);
        transactions.MapPut("{id:guid}", UpdateTransaction);
        transactions.MapDelete("{id:guid}", DeleteTransaction);
    }

    private static async Task<IResult> GetTransaction(Guid id, [FromServices] LedgerDbContext db) {
        var transaction = await db.Transactions.FindAsync(id);

        return transaction is null ? NotFound() : Ok(transaction);
    }

    private static async Task<IResult> ListTransactions([FromServices] LedgerDbContext db) {
        var transactions = await db.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return Ok(transactions.Select(TableTransaction.FromTransaction));
    }

    private static async Task<IResult> CreateTransaction(
        [FromBody]
        TransactionRequest newTransaction,
        [FromServices]
        LedgerDbContext db,
        [FromServices]
        IMapper mapper
    ) => await CreateNewTransaction(newTransaction, db.Transactions, mapper)
        .ThenAsync(
            async transaction => {
                db.Transactions.Add(transaction!);
                await db.SaveChangesAsync();

                return Created($"/api/transactions/{transaction!.Id}", transaction);
            }
        )
        .When(
            ok: result => result,
            error: err => err.ToHttpResult()
        );

    private static async Task<IResult> UpdateTransaction(
        Guid id,
        [FromBody]
        TransactionRequest transactionUpdate,
        [FromServices]
        LedgerDbContext db,
        [FromServices]
        IMapper mapper
    ) => await ValidateTransaction(transactionUpdate)
        .ThenAsync(
            async type => {
                var transaction = await db.Transactions.FindAsync(id);

                if (transaction is null) {
                    return NotFound();
                }

                transaction.Type = type;
                mapper.Map(transactionUpdate, transaction);

                db.Update(transaction);
                await db.SaveChangesAsync();

                return Ok(transaction);
            }
        )
        .When(
            ok: result => result,
            error => error.ToHttpResult()
        );

    private static async Task<IResult> DeleteTransaction(Guid id, [FromServices] LedgerDbContext db) {
        var transaction = await db.Transactions.FindAsync(id);

        if (transaction is null) {
            return NoContent();
        }

        transaction.IsDeleted = true;
        db.Update(transaction);
        await db.SaveChangesAsync();

        return NoContent();
    }
}

public record TransactionRequest(
    string Type,
    string Description,
    DateOnly Date,
    decimal Amount,
    Guid? SourceAccountId,
    Guid? DestinationAccountId,
    string? Category,
    bool SkipHashCheck = false
);

public class TransactionProfile: Profile {
    public TransactionProfile() {
        CreateMap<TransactionRequest, Transaction>()
            .ForMember(t => t.Type, opt => opt.Ignore());
    }
}

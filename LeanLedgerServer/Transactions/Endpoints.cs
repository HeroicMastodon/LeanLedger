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

    private static async Task<IResult> ListTransactions(
        [AsParameters]
        QueryByMonth byMonth,
        [FromServices]
        LedgerDbContext db
    ) {
        var query = db.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .AsQueryable();

        if (byMonth is { Month: not null, Year: not null }) {
            query = query.Where(t => t.Date.Month == byMonth.Month && t.Date.Year == byMonth.Year);
        }

        var transactions = await query
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
    ) {
        var rules = await db.Rules.ToListAsync();

        return await CreateNewTransaction(newTransaction, db.Transactions, mapper, rules)
            .ThenAsync(
                async transaction => {
                    db.Transactions.Add(transaction!);
                    await db.SaveChangesAsync();

                    return Created($"/api/transactions/{transaction!.Id}", transaction);
                }
            )
            .Map(
                ok: result => result,
                error: err => err.ToHttpResult()
            );
    }

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
        .Map(
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
    bool SkipHashCheck = false,
    DateOnly? ImportDate = null
) {
    public string CreateHash() => Hashing.HashString(
        $"{Type}{Description}{Date}{Amount}{SourceAccountId}{DestinationAccountId}{Category}"
    );
}

public class TransactionProfile: Profile {
    public TransactionProfile() {
        CreateMap<TransactionRequest, Transaction>()
            .ForMember(t => t.Type, opt => opt.Ignore());
    }
}

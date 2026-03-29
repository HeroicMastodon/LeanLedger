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
        transactions.MapAllocationEndpoints();
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
                async creation => {
                    var transaction = creation!.Transaction;

                    // Add transaction and any pending allocations atomically
                    db.Transactions.Add(transaction);

                    if (creation.PendingAllocations is not null && creation.PendingAllocations.Count > 0) {
                        if (transaction.IsDeleted) return BadRequest("Cannot add allocations to deleted transaction");
                        if (transaction.Type == TransactionType.Transfer) return BadRequest("Cannot add allocations to transfer transaction");

                        var pendingSum = creation.PendingAllocations.Sum(p => p.Amount);
                        if (pendingSum > transaction.Amount) return BadRequest("Allocations exceed transaction amount");

                        foreach (var p in creation.PendingAllocations) {
                            var piggy = await db.PiggyBanks.FindAsync(p.PiggyBankId);
                            if (piggy is null) return BadRequest($"Piggy bank {p.PiggyBankId} not found");
                            if (piggy.Closed) return BadRequest($"Piggy bank {p.PiggyBankId} is closed");

                            var alloc = new LeanLedgerServer.PiggyBanks.PiggyAllocation {
                                Id = Guid.NewGuid(),
                                TransactionId = transaction.Id,
                                PiggyBankId = p.PiggyBankId,
                                Amount = p.Amount
                            };

                            await db.AddAsync(alloc);
                        }
                    }

                    await db.SaveChangesAsync();

                    return Created($"/api/transactions/{transaction.Id}", transaction);
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

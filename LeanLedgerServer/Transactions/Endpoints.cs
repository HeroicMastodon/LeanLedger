namespace LeanLedgerServer.Transactions;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Results;

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

        return Ok(
            new {
                Transactions = transactions.Select(
                    t => new {
                        t.Id,
                        t.Description,
                        t.Amount,
                        t.Date,
                        t.Category,
                        SourceAccount = new { t.SourceAccount?.Id, t.SourceAccount?.Name },
                        DestinationAccount = new { t.DestinationAccount?.Id, t.DestinationAccount?.Name },
                    }
                )
            }
        );
    }

    private static async Task<IResult> CreateTransaction(
        [FromBody]
        TransactionRequest newTransaction,
        [FromServices]
        LedgerDbContext db
    ) {
        if (!Enum.TryParse<TransactionType>(newTransaction.Type, out var transactionType)) {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid type",
                detail: $"Transaction type {newTransaction.Type} is invalid"
            );
        }

        var transactionHash = Guid.NewGuid().ToString();

        if (!newTransaction.SkipHashCheck) {
            var existingHash = await db.Transactions.FirstOrDefaultAsync(t => t.UniqueHash == transactionHash);

            if (existingHash is not null) {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Transaction Hash Conflict",
                    detail: $"Transaction already exists: {existingHash.UniqueHash}",
                    extensions: new Dictionary<string, object?>() { { "hash", transactionHash }, { "existingTransaction", existingHash } }
                );
            }
        }

        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = transactionHash,
            Type = transactionType,
            Date = newTransaction.Date,
            Amount = newTransaction.Amount,
            Description = newTransaction.Description,
            SourceAccountId = newTransaction.SourceAccountId,
            DestinationAccountId = newTransaction.DestinationAccountId,
            Category = newTransaction.Category,
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        return Created($"/api/transactions/{transaction.Id}", transaction);
    }

    private static async Task<IResult> UpdateTransaction(
        Guid id,
        [FromBody]
        TransactionRequest transactionUpdate,
        [FromServices]
        LedgerDbContext db
    ) {
        if (!Enum.TryParse<TransactionType>(transactionUpdate.Type, out var transactionType)) {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid type",
                detail: $"Transaction type {transactionUpdate.Type} is invalid"
            );
        }

        var transaction = await db.Transactions.FindAsync(id);

        if (transaction is null) {
            return NotFound();
        }

        transaction.Type = transactionType;
        transaction.Date = transactionUpdate.Date;
        transaction.Amount = transactionUpdate.Amount;
        transaction.Description = transactionUpdate.Description;
        transaction.SourceAccountId = transactionUpdate.SourceAccountId;
        transaction.DestinationAccountId = transactionUpdate.DestinationAccountId;
        transaction.Category = transactionUpdate.Category;

        db.Update(transaction);
        await db.SaveChangesAsync();

        return Ok(transaction);
    }

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

    private record TransactionRequest(
        string Type,
        string Description,
        DateOnly Date,
        decimal Amount,
        Guid? SourceAccountId,
        Guid? DestinationAccountId,
        string? Category,
        bool SkipHashCheck = false
    );
}

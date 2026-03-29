namespace LeanLedgerServer.Transactions;

using System.Collections.Immutable;
using Common;
using LeanLedgerServer.PiggyBanks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Results;

public static class AllocationEndpoints {
    public static void MapAllocationEndpoints(this IEndpointRouteBuilder endpoints) {
        var tx = endpoints.MapGroup("transactions");
        tx.MapGet("{transactionId:guid}/allocations", ListAllocations);
        tx.MapPost("{transactionId:guid}/allocations", CreateAllocation);
        tx.MapPut("{transactionId:guid}/allocations/{allocationId:guid}", UpdateAllocation);
        tx.MapDelete("{transactionId:guid}/allocations/{allocationId:guid}", DeleteAllocation);
    }

    private static async Task<IResult> ListAllocations(Guid transactionId, [FromServices] LedgerDbContext db) {
        var tx = await db.Transactions.FindAsync(transactionId);
        if (tx is null) return NotFound();

        var allocs = await db.PiggyAllocations
            .Where(a => a.TransactionId == transactionId)
            .Join(db.PiggyBanks, a => a.PiggyBankId, p => p.Id, (a, p) => new { a.Id, a.Amount, Piggy = p.Name, a.PiggyBankId })
            .ToListAsync();

        return Ok(allocs.Select(a => new { id = a.Id, amount = a.Amount, piggyBankId = a.PiggyBankId, piggyName = a.Piggy }));
    }

    private static async Task<IResult> CreateAllocation(Guid transactionId, [FromBody] AllocationRequest req, [FromServices] LedgerDbContext db) {
        var tx = await db.Transactions.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == transactionId);
        if (tx is null) return NotFound();
        if (tx.IsDeleted) return BadRequest("Cannot add allocation to deleted transaction");
        if (tx.Type == TransactionType.Transfer) return BadRequest("Cannot add allocation to transfer transaction");

        var piggy = await db.PiggyBanks.FindAsync(req.PiggyBankId);
        if (piggy is null) return BadRequest("Piggy bank not found");
        if (piggy.Closed) return BadRequest("Cannot add allocation to closed piggy bank");

        if (req.Amount <= 0) return BadRequest("Amount must be greater than zero");

        var existing = await db.PiggyAllocations.Where(a => a.TransactionId == transactionId).SumAsync(a => a.Amount);
        if (existing + req.Amount > tx.Amount) return BadRequest("Allocations exceed transaction amount");

        var alloc = new PiggyAllocation {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            PiggyBankId = req.PiggyBankId,
            Amount = req.Amount
        };

        await db.AddAsync(alloc);
        await db.SaveChangesAsync();

        return Created($"/api/transactions/{transactionId}/allocations/{alloc.Id}", alloc);
    }

    private static async Task<IResult> UpdateAllocation(Guid transactionId, Guid allocationId, [FromBody] AllocationRequest req, [FromServices] LedgerDbContext db) {
        var tx = await db.Transactions.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == transactionId);
        if (tx is null) return NotFound();
        if (tx.IsDeleted) return BadRequest("Cannot update allocation on deleted transaction");
        if (tx.Type == TransactionType.Transfer) return BadRequest("Cannot update allocation on transfer transaction");

        var alloc = await db.PiggyAllocations.FindAsync(allocationId);
        if (alloc is null) return NotFound();
        if (alloc.TransactionId != transactionId) return BadRequest("Allocation does not belong to transaction");

        var piggy = await db.PiggyBanks.FindAsync(req.PiggyBankId);
        if (piggy is null) return BadRequest("Piggy bank not found");
        if (piggy.Closed) return BadRequest("Cannot add allocation to closed piggy bank");

        if (req.Amount <= 0) return BadRequest("Amount must be greater than zero");

        var existing = await db.PiggyAllocations.Where(a => a.TransactionId == transactionId && a.Id != allocationId).SumAsync(a => a.Amount);
        if (existing + req.Amount > tx.Amount) return BadRequest("Allocations exceed transaction amount");

        alloc.PiggyBankId = req.PiggyBankId;
        alloc.Amount = req.Amount;

        db.Update(alloc);
        await db.SaveChangesAsync();

        return Ok(alloc);
    }

    private static async Task<IResult> DeleteAllocation(Guid transactionId, Guid allocationId, [FromServices] LedgerDbContext db) {
        var alloc = await db.PiggyAllocations.FindAsync(allocationId);
        if (alloc is null) return NotFound();
        if (alloc.TransactionId != transactionId) return BadRequest("Allocation does not belong to transaction");

        db.Remove(alloc);
        await db.SaveChangesAsync();

        return NoContent();
    }
}

public record AllocationRequest(Guid PiggyBankId, decimal Amount);

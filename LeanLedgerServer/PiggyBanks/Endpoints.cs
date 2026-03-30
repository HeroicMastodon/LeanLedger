namespace LeanLedgerServer.PiggyBanks;

using System.Collections.Immutable;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;
using static Results;

public static class Endpoints {
    public static void MapPiggyBanks(this IEndpointRouteBuilder endpoints) {
        var piggies = endpoints.MapGroup("piggy-banks");

        piggies.MapGet("", ListPiggyBanks);
        piggies.MapGet("{id:guid}", GetPiggyBank);
        piggies.MapPost("", CreatePiggyBank);
        piggies.MapPut("{id:guid}", UpdatePiggyBank);
        piggies.MapDelete("{id:guid}", ClosePiggyBank);
    }

    private static async Task<IResult> ListPiggyBanks(
        [AsParameters] QueryByMonth byMonth,
        [FromServices] LedgerDbContext db
    ) {
        // compute balances as of byMonth (cumulative)
        var piggies = await db.PiggyBanks
            .AsQueryable()
            .Where(pb => !pb.Closed)
            .Select(pb => new {
                pb.Id,
                pb.Name,
                pb.InitialBalance,
                pb.BalanceTarget,
                pb.Closed
            })
            .ToListAsync();

        var results = new List<object>();

        foreach (var pb in piggies) {
            var allocSum = await db.PiggyAllocations
                .Where(a => a.PiggyBankId == pb.Id)
                .Join(db.Transactions, a => a.TransactionId, t => t.Id, (a, t) => new { a.Amount, t.Date, t.IsDeleted })
                .Where(x => !x.IsDeleted)
                .Where(x => (byMonth.Month != null && byMonth.Year != null)
                    ? (x.Date.Year < byMonth.Year || (x.Date.Year == byMonth.Year && x.Date.Month <= byMonth.Month))
                    : true)
                .SumAsync(x => x.Amount);

            var balance = pb.InitialBalance + allocSum;
            var progress = pb.BalanceTarget is null ? null : (decimal?)(balance / pb.BalanceTarget.Value * 100m);

            results.Add(new {
                id = pb.Id,
                name = pb.Name,
                initialBalance = pb.InitialBalance,
                balanceTarget = pb.BalanceTarget,
                balance,
                progressPercent = progress,
                closed = pb.Closed
            });
        }

        return Ok(results);
    }

    private static async Task<IResult> GetPiggyBank(Guid id, [AsParameters] QueryByMonth byMonth, [FromServices] LedgerDbContext db) {
        var pb = await db.PiggyBanks.FindAsync(id);

        if (pb is null)
            return NotFound();

        var allocs = await db.PiggyAllocations
            .Where(a => a.PiggyBankId == pb.Id)
            .Include(a => a.Transaction)
                .ThenInclude(t => t.SourceAccount)
            .Include(a => a.Transaction)
                .ThenInclude(t => t.DestinationAccount)
            .Where(a => !a.Transaction.IsDeleted)
            .Where(a => (byMonth.Month != null && byMonth.Year != null)
                ? (a.Transaction.Date.Year < byMonth.Year || (a.Transaction.Date.Year == byMonth.Year && a.Transaction.Date.Month <= byMonth.Month))
                : true)
            .Select(a => new {
                id = a.Id,
                allocationAmount = a.Amount,
                transactionId = a.TransactionId,
                date = a.Transaction.Date,
                description = a.Transaction.Description,
                transactionAmount = a.Transaction.Amount,
                category = a.Transaction.Category,
                type = a.Transaction.Type,
                sourceAccount = a.Transaction.SourceAccount == null ? null : new { id = a.Transaction.SourceAccount.Id, name = a.Transaction.SourceAccount.Name },
                destinationAccount = a.Transaction.DestinationAccount == null ? null : new { id = a.Transaction.DestinationAccount.Id, name = a.Transaction.DestinationAccount.Name }
            })
            .ToListAsync();

        var balance = pb.InitialBalance + allocs.Sum(a => a.allocationAmount);
        var progress = pb.BalanceTarget is null ? null : (decimal?)(balance / pb.BalanceTarget.Value * 100m);

        return Ok(new {
            id = pb.Id,
            name = pb.Name,
            initialBalance = pb.InitialBalance,
            balanceTarget = pb.BalanceTarget,
            balance,
            progressPercent = progress,
            closed = pb.Closed,
            allocations = allocs
        });
    }

    private static async Task<IResult> CreatePiggyBank([FromBody] PiggyBankRequest req, [FromServices] LedgerDbContext db) {
        var pb = new PiggyBank {
            Id = Guid.NewGuid(),
            Name = req.Name,
            InitialBalance = req.InitialBalance ?? 0m,
            BalanceTarget = req.BalanceTarget
        };

        await db.AddAsync(pb);
        await db.SaveChangesAsync();

        return Created($"/api/piggy-banks/{pb.Id}", pb);
    }

    private static async Task<IResult> UpdatePiggyBank(Guid id, [FromBody] PiggyBankRequest req, [FromServices] LedgerDbContext db) {
        var pb = await db.PiggyBanks.FindAsync(id);

        if (pb is null)
            return NotFound();

        pb.Name = req.Name;
        pb.InitialBalance = req.InitialBalance ?? pb.InitialBalance;
        pb.BalanceTarget = req.BalanceTarget;

        db.Update(pb);
        await db.SaveChangesAsync();

        return Ok(pb);
    }

    private static async Task<IResult> ClosePiggyBank(Guid id, [FromServices] LedgerDbContext db) {
        var pb = await db.PiggyBanks.FindAsync(id);

        if (pb is null)
            return NotFound();

        pb.Closed = true;
        db.Update(pb);
        await db.SaveChangesAsync();

        return NoContent();
    }
}

public record PiggyBankRequest(string Name, decimal? InitialBalance, decimal? BalanceTarget);

namespace LeanLedgerServer.PiggyBanks;

using LeanLedgerServer.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PiggyBanksController(LedgerDbContext db): ControllerBase {
    // TODO: Big Problem
    // Need to take into account the closed date
    // An account and its allocations should show up for the duration it was active
    [HttpGet]
    public async Task<IActionResult> ListPiggyBanks([FromQuery] QueryByMonth byMonth) {
        // compute balances as of byMonth (cumulative)
        var piggies = await db.PiggyBanks
            .AsQueryable()
            .Where(pb => !pb.Closed)
            .Where(pb => byMonth.Month == null
                || byMonth.Year == null
                || pb.OpenDate.Year < byMonth.Year
                || (
                    pb.OpenDate.Year == byMonth.Year
                    && pb.OpenDate.Month <= byMonth.Month
                 )
            )
            .Select(pb => new {
                pb.Id,
                pb.Name,
                pb.TargetBalance,
                pb.Closed
            })
            .ToListAsync();

        var results = new List<object>();

        foreach (var piggyBank in piggies) {
            var balance = await db.PiggyBankEntries
                .Where(a => a.PiggyBankId == piggyBank.Id)
                .Where(e => e.Transaction == null || !e.Transaction.IsDeleted)
                .Where(e => byMonth.Month == null
                    || byMonth.Year == null
                    || e.Date.Year < byMonth.Year
                    || (
                        e.Date.Year == byMonth.Year
                        && e.Date.Month <= byMonth.Month
                    )
                )
                .SumAsync(x => x.Amount);

            // TODO: this may need to be extracted
            var progress = piggyBank.TargetBalance is null
                ? null
                : (decimal?)(balance / piggyBank.TargetBalance.Value * 100m);

            results.Add(new {
                piggyBank.Id,
                piggyBank.Name,
                piggyBank.TargetBalance,
                piggyBank.Closed,
                Balance = balance,
                Progress = progress,
            });
        }

        // TODO: this may not work
        var test = await db.PiggyBankEntries
            .Include(e => e.PiggyBank)
            .Where(e => e.PiggyBank!.Closed)
            .Where(e => e.Transaction == null || !e.Transaction.IsDeleted)
            // TODO: This is probably redundant
            .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || e.PiggyBank!.OpenDate.Year < byMonth.Year
                || (
                    e.PiggyBank.OpenDate.Year == byMonth.Year
                    && e.PiggyBank.OpenDate.Month <= byMonth.Month
                )
            )
            // TODO: Need to remove: We need to sum all the allocations, not just for this month
            .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || e.Date.Year < byMonth.Year
                || (
                    e.Date.Year == byMonth.Year
                    && e.Date.Month <= byMonth.Month
                )
            )
            .GroupBy(e => e.PiggyBank)
            .Select(g => new {
                g.Key!.Id,
                g.Key.Name,
                g.Key.TargetBalance,
                g.Key.Closed,
                Balance = g.Sum(e => e.Amount),
                Progress = g.Key.TargetBalance != null
                    ? (decimal?)(g.Sum(e => e.Amount) / g.Key.TargetBalance.Value * 100m)
                    : null
            })
            .ToListAsync();

        return Ok(results);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetPiggyBank(Guid id, [FromQuery] QueryByMonth byMonth) {
        var piggyBank = await db.PiggyBanks.FindAsync(id);

        if (piggyBank is null) {
            return NotFound();
        }

        var entries = await db.PiggyBankEntries
            .Where(e => e.PiggyBankId == piggyBank.Id)
            .Include(e => e.Transaction)
                .ThenInclude(t => t!.SourceAccount)
            .Where(e => !e.Transaction!.IsDeleted)
            .Select(e => new {
                e.Id,
                e.Date,
                e.Amount,
                e.Description,
                PiggyBank = new {
                    e.PiggyBank!.Id,
                    e.PiggyBank.Name
                },
                Transaction = e.TransactionId != null
                    ? new {
                        e.TransactionId,
                        e.Transaction!.Description,
                    }
                    : null,
            })
            .ToListAsync();

        var balance = entries.Sum(e => e.Amount);
        var progress = piggyBank.TargetBalance is null
            ? null
            : (decimal?)(balance / piggyBank.TargetBalance.Value * 100m);

        return Ok(new {
            piggyBank.Id,
            piggyBank.Name,
            piggyBank.TargetBalance,
            piggyBank.Closed,
            Balance = balance,
            ProgressPercent = progress,
            Entries = entries.Where(e => byMonth.Month == null
                || byMonth.Year == null
                || e.Date.Year < byMonth.Year
                || (
                    e.Date.Year == byMonth.Year
                    && e.Date.Month <= byMonth.Month
                )
            ).ToList()
        });

    }

    // Create a new piggy bank
    [HttpPost]
    public async Task<IActionResult> CreatePiggyBank([FromBody] PiggyBankRequest req) {
        var piggyBank = new PiggyBank {
            Name = req.Name,
            TargetBalance = req.TargetBalance,
            OpenDate = req.OpenDate,
            CloseDate = req.ClosedDate,
        };

        _ = await db.AddAsync(piggyBank);
        _ = await db.SaveChangesAsync();

        return Created($"/api/piggy-banks/{piggyBank.Id}", piggyBank);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePiggyBank(Guid id, [FromBody] PiggyBankRequest req) {
        var piggyBank = await db.PiggyBanks.FindAsync(id);

        if (piggyBank is null) {
            return NotFound();
        }

        piggyBank.Name = req.Name;
        piggyBank.TargetBalance = req.TargetBalance;
        piggyBank.OpenDate = req.OpenDate;
        piggyBank.CloseDate = req.ClosedDate;

        _ = db.Update(piggyBank);
        _ = await db.SaveChangesAsync();

        return Ok(piggyBank);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePiggyBank(Guid id) {
        _ = await db.PiggyBanks.Where(pb => pb.Id == id).ExecuteDeleteAsync();

        return NoContent();
    }
}


public record PiggyBankRequest(
    string Name,
    decimal? TargetBalance,
    DateOnly OpenDate,
    DateOnly? ClosedDate
);


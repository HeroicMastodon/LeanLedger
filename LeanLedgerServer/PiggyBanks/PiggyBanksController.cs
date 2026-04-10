namespace LeanLedgerServer.PiggyBanks;

using LeanLedgerServer.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PiggyBanksController(LedgerDbContext db): ControllerBase {
    [HttpGet]
    public async Task<IActionResult> ListPiggyBanks([FromQuery] QueryByMonth byMonth) {
        // TODO: this may not work
        var results = await db.PiggyBankEntries
            .Include(e => e.PiggyBank)
            .Where(e => e.Transaction == null || !e.Transaction.IsDeleted)
            .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || (
                    (
                        e.PiggyBank!.OpenDate.Year < byMonth.Year
                        || (e.PiggyBank!.OpenDate.Year == byMonth.Year
                            && e.PiggyBank!.OpenDate.Month <= byMonth.Month
                        )
                     ) && (e.PiggyBank!.CloseDate == null || (
                            e.PiggyBank!.CloseDate.Value.Month >= byMonth.Month
                            && e.PiggyBank!.CloseDate.Value.Year >= byMonth.Year
                    ))
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

        var entriesQuery = db.PiggyBankEntries
            .Where(e => e.PiggyBankId == piggyBank.Id)
            .Where(e => !e.Transaction!.IsDeleted);
        var balance = await entriesQuery.SumAsync(e => e.Amount);
        var progress = piggyBank.TargetBalance is null
            ? null
            : (decimal?)(balance / piggyBank.TargetBalance.Value * 100m);

        var entries = await entriesQuery
            .Include(e => e.Transaction)
            .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || e.Date.Year < byMonth.Year
                || (
                    e.Date.Year == byMonth.Year
                    && e.Date.Month <= byMonth.Month
                )
            )
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

        return Ok(new {
            piggyBank.Id,
            piggyBank.Name,
            piggyBank.TargetBalance,
            piggyBank.Closed,
            Balance = balance,
            ProgressPercent = progress,
            Entries = entries
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


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
        var results = await db.GetPiggyMetrics(byMonth).ToListAsync();

        return Ok(results);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPiggyBank(Guid id, [FromQuery] QueryByMonth byMonth) {
        var piggyBank = await db.PiggyBanks.FindAsync(id);

        if (piggyBank is null) {
            return NotFound();
        }

        var entriesQuery = db.PiggyBankEntries
            .Where(e => e.PiggyBankId == piggyBank.Id);
        var balance = await entriesQuery.SumAsync(e => e.Amount);
        var progress = piggyBank.TargetBalance is null
            ? null
            : (decimal?)(balance / piggyBank.TargetBalance.Value * 100m);

        var entries = await entriesQuery
            .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || (
                    e.Date.Year == byMonth.Year
                    && e.Date.Month == byMonth.Month
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
        var deleteCount = await db.PiggyBanks.Where(pb => pb.Id == id).ExecuteDeleteAsync();

        if (deleteCount < 1) {
            return NotFound();
        }

        return NoContent();
    }


    // TODO: create piggy bank entry CRUD
    [HttpGet("{id:guid}/entries")]
    public async Task<IActionResult> ListPiggyBankEntries(Guid id, [FromQuery] QueryByMonth byMonth) {
        if (!byMonth.IsValidQuery()) {
            return BadRequest("Month and Year query parameters are required");
        }

        var entries = await db.PiggyBankEntries
            .Where(e => e.PiggyBankId == id)
            .Where(e => e.Date.Year == byMonth.Year && e.Date.Month == byMonth.Month)
            .ToArrayAsync();

        return Ok(entries);
    }

    [HttpPost("{id:guid}/entries")]
    public async Task<IActionResult> CreatePiggyBankEntry(
        Guid id,
        [FromBody] PiggyBankEntryRequest req
    ) {
        var piggyBank = await db.PiggyBanks.FindAsync(id);

        if (piggyBank is null) {
            return NotFound();
        }

        var newEntry = new PiggyBankEntry {
            PiggyBankId = id,
            Amount = req.Amount,
            Date = req.Date,
            Description = req.Description ?? "",
        };

        _ = await db.PiggyBankEntries.AddAsync(newEntry);
        _ = await db.SaveChangesAsync();

        return Created();
    }

    [HttpPut("{piggyBankId:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> UpdatePiggyBankEntry(
        Guid piggyBankId,
        Guid entryId,
        [FromBody] PiggyBankEntryRequest req
    ) {
        var entry = await db.PiggyBankEntries
            .Where(e => e.PiggyBankId == piggyBankId && e.Id == entryId)
            .FirstOrDefaultAsync();

        if (entry is null) {
            return NotFound();
        }

        entry.Amount = req.Amount;
        entry.Date = req.Date;
        entry.Description = req.Description ?? entry.Description;
        entry.PiggyBankId = req.PiggyBankId ?? entry.PiggyBankId;

        _ = db.Update(entry);
        _ = await db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{piggyBankId:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> DeletePiggyBankEntry(Guid piggyBankId, Guid entryId) {
        var deleteCount = await db.PiggyBankEntries
            .Where(e => e.PiggyBankId == piggyBankId && e.Id == entryId)
            .ExecuteDeleteAsync();

        if (deleteCount < 1) {
            return NotFound();
        }

        return NoContent();
    }
}


public record PiggyBankRequest(
    string Name,
    decimal? TargetBalance,
    DateOnly OpenDate,
    DateOnly? ClosedDate
);
public record PiggyBankEntryRequest(decimal Amount, DateOnly Date, string? Description, Guid? PiggyBankId);

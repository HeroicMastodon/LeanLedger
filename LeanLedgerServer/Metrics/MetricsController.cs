namespace LeanLedgerServer.Metrics;

using Accounts;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class MetricsController(
    LedgerDbContext dbContext
): Controller {
    [HttpGet("net-worth")]
    public async Task<IActionResult> GetNetWorth(
        [FromQuery] QueryByMonth byMonth
    ) {
        var accounts = await dbContext.Accounts
            .Where(a => a.Active)
            .Where(a => a.IncludeInNetWorth)
            .ToListAsync();

        var netWorth = 0m;
        foreach (var account in accounts) {
            var accountBalance = await AccountFunctions.QueryBalanceChangeByMonth(
                byMonth,
                dbContext,
                account
            );

            netWorth += accountBalance;
        }

        var shouldGetEverything = !byMonth.IsValidQuery();
        var totals = await dbContext.Accounts
            .Include(a => a.Withdrawls)
            .Include(a => a.Deposits)
            .Where(a => a.Active)
            .Where(a => a.IncludeInNetWorth)
            .Select(
                a => new {
                    totalIncome = a.Deposits
                        .Where(
                            d => shouldGetEverything
                                 || (d.Date.Year == byMonth.Year && d.Date.Month == byMonth.Month)
                        )
                        .Sum(d => d.Amount),
                    totalExpenses = a.Withdrawls
                        .Where(
                            d => shouldGetEverything
                                 || (d.Date.Year == byMonth.Year && d.Date.Month == byMonth.Month)
                        )
                        .Sum(d => d.Amount),
                }
            )
            .ToListAsync();

        return Ok(
            new {
                netWorth,
                income = totals.Sum(t => t.totalIncome),
                expenses = totals.Sum(t => t.totalExpenses),
            }
        );
    }
}

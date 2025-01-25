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
        [FromQuery]
        QueryByMonth byMonth
    ) {
        var netWorth = await QueryNetWorth(byMonth);

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

    private async Task<decimal> QueryNetWorth(QueryByMonth byMonth) {
        var accounts = await dbContext.Accounts
            .Where(a => a.Active)
            .Where(a => a.IncludeInNetWorth)
            .ToListAsync();

        var netWorth = 0m;
        foreach (var account in accounts) {
            var accountBalance = await AccountFunctions.QueryBalanceByMonth(
                byMonth,
                dbContext,
                account
            );

            netWorth += accountBalance;
        }

        return netWorth;
    }

    // Leaving this endpoint in case I ever want to try time series data again
    [HttpGet("time-series/net-worth")]
    public async Task<IActionResult> GetNetWorthChange(
        [FromQuery]
        QueryByMonth byMonth
    ) {
        var lastMonthsNetWorth = await QueryNetWorth(byMonth.Decrement());
        var incomeByDay = await dbContext.Accounts
            .Where(a => a.Active)
            .Where(a => a.IncludeInNetWorth)
            .Include(a => a.Deposits)
            .SelectMany(a => a.Deposits)
            .Where(t => t.Date.Month == byMonth.Month && t.Date.Year == byMonth.Year)
            .GroupBy(t => t.Date.Day)
            .Select(group => new ByDayMetric(group.Key, group.Sum(t => t.Amount)))
            .AsSplitQuery()
            .ToListAsync();
        var expensesByDay = await dbContext.Accounts
            .Where(a => a.Active)
            .Where(a => a.IncludeInNetWorth)
            .Include(a => a.Withdrawls)
            .SelectMany(a => a.Withdrawls)
            .Where(t => t.Date.Month == byMonth.Month && t.Date.Year == byMonth.Year)
            .GroupBy(t => t.Date.Day)
            .Select(group => new ByDayMetric(group.Key, group.Sum(t => t.Amount * -1)))
            .AsSplitQuery()
            .ToListAsync();
        var daysInMonth = DateTime.DaysInMonth(byMonth.Year.Value, byMonth.Month.Value);
        foreach (var day in Enumerable.Range(1, daysInMonth)) {
            if (incomeByDay.All(d => d.Day != day)) {
                incomeByDay.Add(new ByDayMetric(day, 0));
            }

            if (expensesByDay.All(e => e.Day != day)) {
                expensesByDay.Add(new ByDayMetric(day, 0));
            }
        }

        incomeByDay = incomeByDay.OrderBy(d => d.Day).ToList();
        expensesByDay = expensesByDay.OrderBy(d => d.Day).ToList();
        var runningTotal = lastMonthsNetWorth;
        var everything = incomeByDay.Zip(expensesByDay)
            .Select(
                pair => {
                    runningTotal += pair.First.Amount + pair.Second.Amount;

                    return pair.First with {
                        Amount = runningTotal
                    };
                }
            )
            .ToArray();

        return Ok(everything);
    }
}

record ByDayMetric(int Day, decimal Amount);

namespace LeanLedgerServer.Metrics;

using System.Collections.Immutable;
using Accounts;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;

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

    [HttpGet("budget")]
    public async Task<IActionResult> GetBudget(
        [FromQuery]
        QueryByMonth byMonth
    ) {
        if (!byMonth.IsValidQuery()) {
            return BadRequest("Must provide both month and year");
        }

        var budget = await dbContext.Budgets
            .FirstOrDefaultAsync(b => b.Month == byMonth.Month && b.Year == byMonth.Year);

        if (budget is null) {
            return NotFound("Budget was not found for the given month and year");
        }

        var categoryNames = budget.CategoryGroups.SelectMany(g => g.Categories.Select(c => c.Category));
        var categorySums = await dbContext.Transactions
            .Where(t => t.Date.Month == budget.Month && t.Date.Year == budget.Year)
            .Where(t => t.Type == TransactionType.Expense)
            .Where(t => t.Category != null)
            .Where(t => categoryNames.Contains(t.Category))
            .GroupBy(t => t.Category)
            .Select(
                group => new {
                    Category = group.Key,
                    Sum = group.Sum(g => g.Amount)
                }
            )
            .ToDictionaryAsync(c => c.Category, c => c.Sum);
        var categoryGroups = budget.CategoryGroups.Select(
            group => {
                var categories = group.Categories.Select(
                        c => new {
                            c.Limit,
                            c.Category,
                            Actual = categorySums.TryGetValue(c.Category, out var sum) ? sum : 0
                        }
                    )
                    .ToImmutableArray();

                return new {
                    group.Name,
                    Categories = categories,
                    Limit = group.Categories.Sum(c => c.Limit),
                    Actual = categories.Sum(c => c.Actual)
                };
            }
        );
        var expectedExpenses = categoryGroups.Sum(c => c.Limit);
        var totalExpenses = await dbContext.Transactions
            .Where(t => t.Date.Month == budget.Month && t.Date.Year == budget.Year)
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        return Ok(
            new {
                budget.ExpectedIncome,
                ExpectedExpenses = expectedExpenses,
                TotalExpenses = totalExpenses,
                LeftToSpend = expectedExpenses - totalExpenses,
            }
        );
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

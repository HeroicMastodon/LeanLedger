namespace LeanLedgerServer.Metrics;

using System.Collections.Immutable;
using Accounts;
using Common;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Transactions;
using LeanLedgerServer.PiggyBanks;

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
        var lastYearsNetWorth = await QueryNetWorth(byMonth with { Year = byMonth.Year - 1 });

        var shouldGetEverything = !byMonth.IsValidQuery();
        var totals = await dbContext.Accounts
            .Include(a => a.Withdrawls)
            .Include(a => a.Deposits)
            .Where(a => a.Active)
            .Where(a => a.IncludeInNetWorth)
            .Select(
                a => new {
                    totalIncome = a.Deposits
                        .Where(d => d.Type != TransactionType.Transfer)
                        .Where(
                            d => shouldGetEverything
                                 || (d.Date.Year == byMonth.Year && d.Date.Month == byMonth.Month)
                        )
                        .Sum(d => d.Amount),
                    totalExpenses = a.Withdrawls
                        .Where(d => d.Type != TransactionType.Transfer)
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
                yearOverYear = netWorth - lastYearsNetWorth,
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

    [HttpGet("net-worth/amount")]
    public async Task<IActionResult> GetNetWorthAmount(
        [FromQuery]
        QueryByMonth byMonth
    ) {
        var netWorth = await QueryNetWorth(byMonth);
        return Ok(new { netWorth });
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
            .ToDictionaryAsync(c => c.Category!, c => c.Sum);
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
        var actualIncome = await dbContext.Transactions
            .Include(t => t.DestinationAccount)
            .Where(t => t.DestinationAccount != null && t.DestinationAccount.IncludeInNetWorth)
            .Where(t => t.Date.Month == budget.Month && t.Date.Year == budget.Year)
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);
        var incomeDifference = actualIncome - budget.ExpectedIncome;

        return Ok(
            new {
                Income = new {
                    Budgeted = budget.ExpectedIncome,
                    Actual = actualIncome,
                    Difference = incomeDifference
                },
                Expenses = new {
                    Budgeted = expectedExpenses,
                    Actual = totalExpenses,
                    Difference = totalExpenses - expectedExpenses
                },
                Change = new {
                    Budgeted = budget.ExpectedIncome - expectedExpenses,
                    Actual = actualIncome - totalExpenses,
                    Difference = actualIncome - totalExpenses - (budget.ExpectedIncome - expectedExpenses)
                }
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
        var daysInMonth = DateTime.DaysInMonth(byMonth.Year!.Value, byMonth.Month!.Value);
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

    [HttpGet("account-trends")]
    public async Task<IActionResult> GetAccountTrends(
        [FromQuery]
        QueryByMonth byMonth
    ) {
        // Need to retrieve networth and change in networth for 3 different trends, Monthly, Quarterly, Yearly
        // Monthly would be the last three months
        // Quarterly would be the last 4 quarters, so 12 months at 3 month intervals
        // Yearly would be the last 3 years
        // need to match this ts type structure when returning:
        //
        // type Trend = {
        //     label: string; (e.g. "Jan" for monthly, "Jan-Mar" for quarterly, "2024" for yearly)
        //     netWorth: number;
        //     change: number;
        // };
        //
        // type TrendType = "yearly" | "monthly" | "quarterly";
        // type AccountTrends = Record<TrendType, Trend[]>;
        // First, we need to create a list of months/quarters/years based on the query month
        // we need an extra month/quarter/year for the change calculation

        var monthlyQueries = new[] {
            byMonth.Decrement(3),
            byMonth.Decrement(2),
            byMonth.Decrement(1),
        }.Select(
            async m => new {
                Query = m,
                NetWorth = await QueryNetWorth(m)
            }
        )
        .ToArray();
        var quarterlyQueries = new[] {
            byMonth.DecrementByQuarter(4),
            byMonth.DecrementByQuarter(3),
            byMonth.DecrementByQuarter(2),
            byMonth.DecrementByQuarter(1),
        }.Select(
                async q => new {
                    Query = q,
                    NetWorth = await QueryNetWorth(q)
                }
            )
            .ToArray();
        var yearlyQueries = new[] {
            byMonth.DecrementByYear(3),
            byMonth.DecrementByYear(2),
            byMonth.DecrementByYear(1),
        }.Select(
                async y => new {
                    Query = y,
                    NetWorth = await QueryNetWorth(y)
                }
            )
        .ToArray();

        var currentMonthNetWorth = await QueryNetWorth(byMonth);
        var monthlyResults = await Task.WhenAll(monthlyQueries);
        var quarterlyResults = await Task.WhenAll(quarterlyQueries);
        var yearlyResults = await Task.WhenAll(yearlyQueries);


        // TODO: simpflify this by extracting a method
        // then calculate the change from the previous month/quarter/year
        var monthlyTrends = monthlyResults
            .Select(
                (result, index) => new {
                    Label = result.Query.MonthName,
                    result.NetWorth,
                    TotalChange = currentMonthNetWorth - result.NetWorth,
                    Change = index == monthlyResults.Length - 1
                        ? currentMonthNetWorth - result.NetWorth
                        : monthlyResults[index + 1].NetWorth - result.NetWorth
                }
            )
            .ToArray();
        var quarterlyTrends = quarterlyResults.Select(
                (result, index) => new {
                    Label = $"{result.Query.Decrement(2).MonthName}-{result.Query.MonthName}",
                    result.NetWorth,
                    TotalChange = currentMonthNetWorth - result.NetWorth,
                    Change = index == quarterlyResults.Length - 1
                        ? currentMonthNetWorth - result.NetWorth
                        : quarterlyResults[index + 1].NetWorth - result.NetWorth
                }
            )
            .ToArray();
        var yearlyTrends = yearlyResults
            .Select(
                (result, index) => new {
                    Label = result.Query.Year?.ToString(CultureInfo.InvariantCulture),
                    result.NetWorth,
                    TotalChange = currentMonthNetWorth - result.NetWorth,
                    Change = index == yearlyResults.Length - 1
                        ? currentMonthNetWorth - result.NetWorth
                        : yearlyResults[index + 1].NetWorth - result.NetWorth
                }
            )
            .ToArray();

        return Ok(new {
            Monthly = monthlyTrends.Reverse(),
            Quarterly = quarterlyTrends.Reverse(),
            Yearly = yearlyTrends.Reverse()
        });
    }

    [HttpGet("piggy-banks")]
    public async Task<IActionResult> GetPiggyBanksMetrics(
        [FromQuery]
        QueryByMonth byMonth
    ) {
        // Retrieve piggy banks
        var lastMonthMetrics = await dbContext.GetPiggyMetrics(byMonth.Decrement()).ToListAsync();
        var thisMonthMetrics = await dbContext.GetPiggyMetrics(byMonth).ToListAsync();
        var thisMonthWithChange = lastMonthMetrics.Join(thisMonthMetrics, m => m.Id, m => m.Id, (lastMonth, thisMonth) => new {
            thisMonth.Id,
            thisMonth.Name,
            thisMonth.Balance,
            thisMonth.TargetBalance,
            thisMonth.Progress,
            Change = thisMonth.Balance - lastMonth.Balance
        });
        var networth = await QueryNetWorth(byMonth);
        var totalBalance = thisMonthWithChange.Sum(m => m.Balance);

        return Ok(new {
            PiggyBanks = thisMonthWithChange,
            PiggyTotal = totalBalance,
            AccountTotal = networth
        });
    }
}

public record ByDayMetric(int Day, decimal Amount);

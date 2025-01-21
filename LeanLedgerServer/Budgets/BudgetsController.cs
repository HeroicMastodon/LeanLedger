namespace LeanLedgerServer.Budgets;

using System.Collections.Immutable;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController(
    LedgerDbContext dbContext,
    IMapper mapper
): Controller {
    [HttpGet("{year:int}/{month:int}")]
    public async Task<IActionResult> GetBudget(int year, int month) {
        if (month is < 1 or > 12) {
            return BadRequest("Month must be a valid month (1-12)");
        }

        var budget = await dbContext.Budgets.FirstOrDefaultAsync(
            b => b.Year == year && b.Month == month
        );

        if (budget is null) {
            var mostRecent = await dbContext.Budgets
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .FirstOrDefaultAsync();

            budget = new Budget() {
                Id = Guid.NewGuid(),
                ExpectedIncome = mostRecent?.ExpectedIncome ?? 0,
                Month = month,
                Year = year,
                CategoryGroups = mostRecent?.CategoryGroups ?? [],
            };

            await dbContext.AddAsync(budget);
            await dbContext.SaveChangesAsync();
        }

        var view = await GetViewForBudget(budget);

        return Ok(view);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] BudgetRequest request) {
        var budget = await dbContext.Budgets.FindAsync(id);

        if (budget is null) {
            return NotFound();
        }

        mapper.Map(request, budget);
        dbContext.Update(budget);
        await dbContext.SaveChangesAsync();
        var view = await GetViewForBudget(budget);

        return Ok(view);
    }

    private async Task<dynamic> GetViewForBudget(Budget budget) {
        var actualIncome = await dbContext.Transactions
            .Include(t => t.DestinationAccount)
            .Where(t => t.DestinationAccount != null && t.DestinationAccount.IncludeInNetWorth)
            .Where(t => t.Date.Month == budget.Month && t.Date.Year == budget.Year)
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var categoryNames = budget.CategoryGroups.SelectMany(g => g.Categories.Select(c => c.Category));
        var categoryQuery = dbContext.Transactions
            .Where(t => t.Date.Month == budget.Month && t.Date.Year == budget.Year)
            .Where(t => t.Type == TransactionType.Expense)
            .Where(t => t.Category != null);

        var categorySums = await  categoryQuery
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
                var categories = group.Categories.Select(c => new {
                    c.Limit,
                    c.Category,
                    Actual = categorySums.TryGetValue(c.Category, out var sum) ? sum : 0
                }).ToImmutableArray();

                return new {
                    group.Name,
                    Categories = categories,
                    Limit = group.Categories.Sum(c => c.Limit),
                    Actual = categories.Sum(c => c.Actual)
                };
            }
        );

        var remainingExpenseTotal = await categoryQuery
            .Where(t => !categoryNames.Contains(t.Category))
            .SumAsync(t => t.Amount);

        return new {
            budget.Id,
            budget.Month,
            budget.Year,
            budget.ExpectedIncome,
            categoryGroups,
            actualIncome,
            remainingExpenseTotal,
        };
    }
}

public record BudgetRequest(
    decimal ExpectedIncome,
    List<BudgetCategoryGroup> CategoryGroups
);

public class BudgetProfile: Profile {
    public BudgetProfile() {
        CreateMap<BudgetRequest, Budget>();
    }
}

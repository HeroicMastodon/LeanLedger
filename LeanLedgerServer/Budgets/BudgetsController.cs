namespace LeanLedgerServer.Budgets;

using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
                Categories = mostRecent?.Categories ?? []
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

        var categoryNames = budget.Categories.Select(c => c.Category);
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
        var categories = budget.Categories.Select(
            c => new {
                c.Category,
                c.Limit,
                Actual = categorySums.TryGetValue(c.Category, out var sum)
                    ? sum
                    : 0
            }
        );

        return new {
            budget.Id,
            budget.Month,
            budget.Year,
            budget.ExpectedIncome,
            categories,
            actualIncome
        };
    }
}

public record BudgetRequest(
    decimal ExpectedIncome,
    List<BudgetCategory> Categories
);

public class BudgetProfile: Profile {
    public BudgetProfile() {
        CreateMap<BudgetRequest, Budget>();
    }
}

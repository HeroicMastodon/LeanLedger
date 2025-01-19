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
                Year = year
            };
            await dbContext.AddAsync(budget);
            await dbContext.SaveChangesAsync();
        }

        var actualIncome = await dbContext.Transactions
            .Include(t => t.DestinationAccount)
            .Where(t => t.DestinationAccount != null && t.DestinationAccount.IncludeInNetWorth)
            .Where(t => t.Date.Month == budget.Month && t.Date.Year == budget.Year)
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        return Ok(new {
            budget.Id,
            budget.Month,
            budget.Year,
            budget.ExpectedIncome,
            actualIncome
        });
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

        return Ok(budget);
    }
}

public record BudgetRequest(
    decimal ExpectedIncome
);

public class BudgetProfile: Profile {
    public BudgetProfile() {
        CreateMap<BudgetRequest, Budget>();
    }
}

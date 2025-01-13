namespace LeanLedgerServer.Budgets;

using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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

        if (budget is not null) {
            return Ok(budget);
        }

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

        return Ok(budget);
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

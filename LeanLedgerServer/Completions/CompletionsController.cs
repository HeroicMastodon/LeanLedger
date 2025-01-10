using Microsoft.AspNetCore.Mvc;

namespace LeanLedgerServer.Completions;

using Automation;
using Common;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CompletionsController(
    LedgerDbContext dbContext
): Controller {
    [HttpGet("descriptions")]
    public async Task<IActionResult> Descriptions(
        [FromQuery(Name = "value")]
        string? value,
        [FromQuery(Name = "condition")]
        string? conditionString,
        [FromQuery(Name = "not")]
        bool not = false
    ) {
        var query = dbContext.Transactions.Where(t => t.Description != null);

        if (Enum.TryParse<RuleCondition>(conditionString, out var condition)) {
            if (value is null && condition is not RuleCondition.Exists) {
                return BadRequest("Value must be provided for all conditions except 'Exists'");
            }

            query = condition switch {
                RuleCondition.StartsWith =>
                    query.Where(t => EF.Functions.Like(t.Description, $"{value}%") != not),
                RuleCondition.EndsWith =>
                    query.Where(t => EF.Functions.Like(t.Description, $"%{value}") != not),
                RuleCondition.IsExactly =>
                    query.Where(t => (t.Description == value) != not),
                RuleCondition.Exists =>
                    query.Where(t => !string.IsNullOrWhiteSpace(t.Description) != not),
                _ => query.Where(t => EF.Functions.Like(t.Description, $"%{value}%") != not)
            };
        } else {
            query = query.Where(t => EF.Functions.Like(t.Description, $"%{value}%") != not);
        }

        var result = query.Select(t => t.Description)
            .Distinct()
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories() {
        return NotFound();
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> Accounts() {
        return NotFound();
    }
}

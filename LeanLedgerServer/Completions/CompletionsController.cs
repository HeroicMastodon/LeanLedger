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
        if (value is null && conditionString != RuleCondition.Exists.ToString()) {
            return BadRequest("Value must be provided for all conditions except 'Exists'");
        }

        var query = dbContext.Transactions
            .Where(t => t.Description != null)
            .Select(t => t.Description);

        query = ApplyCondition(query, conditionString, value, not);

        var result = await query
            .Distinct()
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(
        [FromQuery(Name = "value")]
        string? value,
        [FromQuery(Name = "condition")]
        string? conditionString,
        [FromQuery(Name = "not")]
        bool not = false
    ) {
        if (value is null && conditionString != RuleCondition.Exists.ToString()) {
            return BadRequest("Value must be provided for all conditions except 'Exists'");
        }

        var query = dbContext.Transactions.Where(t => t.Category != null).Select(t => t.Category);
        query = ApplyCondition(query, conditionString, value, not);

        var result = await query
            .Distinct()
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> Accounts(
        [FromQuery(Name = "value")]
        string? value,
        [FromQuery(Name = "condition")]
        string? conditionString,
        [FromQuery(Name = "not")]
        bool not = false
    ) {
        if (value is null && conditionString != RuleCondition.Exists.ToString()) {
            return BadRequest("Value must be provided for all conditions except 'Exists'");
        }

        IQueryable<string?> query = dbContext.Accounts.Select(a => a.Name);
        query = ApplyCondition(query, conditionString, value, not);

        var result = await query
            .Distinct()
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }

    private static IQueryable<string?> ApplyCondition(
        IQueryable<string?> query,
        string? conditionString,
        string? value,
        bool not
    ) {
        if (!Enum.TryParse<RuleCondition>(conditionString, out var condition)) {
            return query.Where(a => EF.Functions.Like(a, $"%{value}%") != not);
        }

        return condition switch {
            RuleCondition.StartsWith =>
                query.Where(a => EF.Functions.Like(a, $"{value}%") != not),
            RuleCondition.EndsWith =>
                query.Where(a => EF.Functions.Like(a, $"%{value}") != not),
            RuleCondition.IsExactly =>
                query.Where(a => (a == value) != not),
            RuleCondition.Exists =>
                query.Where(a => !string.IsNullOrWhiteSpace(a) != not),
            _ => query.Where(a => EF.Functions.Like(a, $"%{value}%") != not)
        };
    }
}

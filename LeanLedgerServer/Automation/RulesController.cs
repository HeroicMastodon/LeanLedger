using Microsoft.AspNetCore.Mvc;

namespace LeanLedgerServer.Automation;

using AutoMapper;
using Common;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class RulesController(
    LedgerDbContext dbContext,
    IMapper mapper
): Controller {
    [HttpGet]
    public async Task<IActionResult> ListRules() {
        var rules = await dbContext.Rules.ToListAsync();
        return Ok(rules);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRule(Guid id) {
        var rule = await dbContext.Rules.FindAsync(id);

        if (rule is null) {
            return NotFound();
        }

        return Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] RuleRequest request) {
        if (request.RuleGroupName is not null && !await dbContext.RuleGroups.AnyAsync(rg => rg.Name == request.RuleGroupName)) {
            return Problem(
                $"No rule group called {request.RuleGroupName} exists",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var newRule = new Rule {
            Id = Guid.NewGuid()
        };
        mapper.Map(request, newRule);
        await dbContext.AddAsync(newRule);
        await dbContext.SaveChangesAsync();

        return Ok(newRule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] RuleRequest request) {
        if (request.RuleGroupName is not null && !await dbContext.RuleGroups.AnyAsync(rg => rg.Name == request.RuleGroupName)) {
            return Problem(
                $"No rule group called {request.RuleGroupName} exists",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var rule = await dbContext.Rules.FindAsync(id);

        if (rule is null) {
            return NotFound();
        }

        mapper.Map(request, rule);
        dbContext.Update(rule);
        await dbContext.SaveChangesAsync();

        return Ok(rule);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRule(Guid id) {
        var rule = await dbContext.Rules.FindAsync(id);

        if (rule is null) {
            return NotFound();
        }

        rule.IsDeleted = true;

        dbContext.Update(rule);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}

public record RuleRequest(
    string Name,
    bool IsStrict,
    List<RuleTrigger> Triggers,
    List<RuleAction> Actions,
    string? RuleGroupName
);

public class RuleProfile: Profile {
    public RuleProfile() {
        CreateMap<RuleRequest, Rule>();
    }
}

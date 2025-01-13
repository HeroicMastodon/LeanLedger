namespace LeanLedgerServer.Automation;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/rule-groups")]
public class RuleGroupsController(
    LedgerDbContext dbContext,
    RuleService ruleService
): Controller {
    [HttpGet]
    public async Task<IActionResult> ListRuleGroups() {
        var ruleGroups = await dbContext.RuleGroups
            .Include(rg => rg.Rules)
            .Select(rg => new RuleGroupView(rg.Name, rg.Rules))
            .ToListAsync();
        var ungroupedRules = await dbContext.Rules
            .Where(r => r.RuleGroupName == null)
            .ToListAsync();

        ruleGroups.Add(new RuleGroupView(null, ungroupedRules));

        return Ok(ruleGroups);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetRuleGroup(string name) {
        var ruleGroup = await dbContext.RuleGroups.Include(rg => rg.Rules).FirstOrDefaultAsync(rg => rg.Name == name);

        if (ruleGroup is null) {
            return NotFound();
        }

        return Ok(ruleGroup);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRuleGroup([FromBody] RuleGroupRequest request) {
        if (string.IsNullOrWhiteSpace(request.Name)) {
            return BadRequest("Name cannot be missing");
        }

        if (
            request.Name.Equals("(ungrouped)", StringComparison.OrdinalIgnoreCase)
            || request.Name.Equals("run", StringComparison.Ordinal)
        ) {
            return BadRequest("Name cannot be '(ungrouped)' or 'run'");
        }

        var newGroup = new RuleGroup {
            Name = request.Name
        };
        await dbContext.AddAsync(newGroup);
        await dbContext.SaveChangesAsync();

        return Ok(newGroup);
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> UpdateRuleGroup(string name, [FromBody] RuleGroupRequest request) {
        var group = await dbContext.RuleGroups.Include(rg => rg.Rules).FirstOrDefaultAsync(rg => rg.Name == name);

        if (group is null) {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name)) {
            return BadRequest("Name cannot be missing");
        }

        if (
            request.Name.Equals("(ungrouped)", StringComparison.OrdinalIgnoreCase)
            || request.Name.Equals("run", StringComparison.Ordinal)
        ) {
            return BadRequest("Name cannot be '(ungrouped)' or 'run'");
        }

        var newGroup = new RuleGroup {
            Name = request.Name
        };
        group.Rules.ForEach(r => r.RuleGroupName = request.Name);

        dbContext.UpdateRange(group.Rules);
        await dbContext.AddAsync(newGroup);
        dbContext.Remove(group);
        await dbContext.SaveChangesAsync();

        return Ok(group);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteRuleGroup(string name) {
        var ruleGroup = await dbContext.RuleGroups.Include(rg => rg.Rules).FirstOrDefaultAsync(rg => rg.Name == name);

        if (ruleGroup is null) {
            return NotFound();
        }

        ruleGroup.Rules.ForEach(
            r => {
                r.RuleGroupName = null;
                r.RuleGroup = null;
            }
        );
        dbContext.UpdateRange(ruleGroup.Rules);
        ruleGroup.Rules = [];
        dbContext.Remove(ruleGroup);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{name}/run")]
    public async Task<IActionResult> RuleRuleGroup(
        string name,
        [FromBody]
        RunRuleRequest request
    ) {
        var ruleGroup = await dbContext.RuleGroups.Include(rg => rg.Rules).FirstOrDefaultAsync(rg => rg.Name == name);

        if (ruleGroup is null) {
            return NotFound();
        }

        var changedCount = 0;
        foreach (var rule in ruleGroup.Rules) {
            changedCount += await ruleService.Run(rule, request.StartDate, request.EndDate);
        }

        return Ok(
            new {
                Count = changedCount
            }
        );
    }
}

public record RuleGroupRequest(string Name);

public record RuleGroupView(string? Name, List<Rule> Rules);

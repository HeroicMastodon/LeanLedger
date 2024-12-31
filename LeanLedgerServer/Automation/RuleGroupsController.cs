namespace LeanLedgerServer.Automation;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/rule-groups")]
public class RuleGroupsController(
    LedgerDbContext dbContext
): Controller {
    [HttpGet]
    public async Task<IActionResult> ListRuleGroups() {
        var ruleGroups = await dbContext.RuleGroups.Include(rg => rg.Rules).ToListAsync();
        var ungroupedRules = await dbContext.Rules.Where(r => r.RuleGroupName == null).ToListAsync();
        var nameToRules = ruleGroups.ToDictionary(rg => rg.Name, rg => rg.Rules);
        nameToRules["(ungrouped)"] = ungroupedRules;

        return Ok(nameToRules);
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

        group.Name = request.Name;
        group.Rules.ForEach(r => r.RuleGroupName = request.Name);

        dbContext.Update(group);
        dbContext.UpdateRange(group.Rules);
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
}

public record RuleGroupRequest(string Name);

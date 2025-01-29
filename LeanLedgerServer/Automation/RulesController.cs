namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Globalization;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Transactions;

[ApiController]
[Route("api/[controller]")]
public class RulesController(
    LedgerDbContext dbContext,
    IMapper mapper,
    RuleService ruleService
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
        var newRuleGroupName = string.IsNullOrWhiteSpace(request.RuleGroupName) ? null : request.RuleGroupName;
        if (newRuleGroupName is not null && !await dbContext.RuleGroups.AnyAsync(rg => rg.Name == newRuleGroupName)) {
            return Problem(
                $"No rule group called {newRuleGroupName} exists",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var newRule = new Rule {
            Id = Guid.NewGuid()
        };
        mapper.Map(request, newRule);
        newRule.RuleGroupName = newRuleGroupName;
        await dbContext.AddAsync(newRule);
        await dbContext.SaveChangesAsync();

        return Ok(newRule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] RuleRequest request) {
        var newRuleGroupName = string.IsNullOrWhiteSpace(request.RuleGroupName) ? null : request.RuleGroupName;
        if (newRuleGroupName is not null && !await dbContext.RuleGroups.AnyAsync(rg => rg.Name == newRuleGroupName)) {
            return Problem(
                $"No rule group called {newRuleGroupName} exists",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var rule = await dbContext.Rules.FindAsync(id);

        if (rule is null) {
            return NotFound();
        }

        mapper.Map(request, rule);
        rule.RuleGroupName = newRuleGroupName;
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

    [HttpGet("{id:guid}/matching")]
    public async Task<IActionResult> GetMatchingTransactions(
        Guid id,
        [FromQuery(Name = "start")]
        DateOnly startDate,
        [FromQuery(Name = "end")]
        DateOnly endDate,
        [FromQuery(Name = "limit")]
        int limit = 5
    ) {
        var rule = await dbContext.Rules.FindAsync(id);

        if (rule is null) {
            return NotFound();
        }

        var results = await ruleService.FindMatchingTransactionsFor(
            rule,
            startDate,
            endDate,
            limit,
            includeAccounts:true
        );

        return Ok(results.Select(TableTransaction.FromTransaction));
    }

    [HttpPost("{id:guid}/run")]
    public async Task<IActionResult> RunRule(
        Guid id,
        [FromBody]
        RunRuleRequest request
    ) {
        var rule = await dbContext.Rules.FindAsync(id);

        if (rule is null) {
            return NotFound();
        }

        var changedCount = await ruleService.Run(rule, request.StartDate, request.EndDate);

        return Ok(
            new {
                Count = changedCount
            }
        );
    }

    [HttpPost("run-all")]
    public async Task<IActionResult> RunAllRules([FromBody] RunRuleRequest request) {
        var rules = await dbContext.Rules.ToListAsync();
        var changedCount = 0;

        foreach (var rule in rules) {
            changedCount += await ruleService.Run(
                rule,
                request.StartDate,
                request.EndDate
            );
        }

        return Ok(
            new {
                Count = changedCount
            }
        );
    }
}

public record RuleRequest(
    string Name,
    bool IsStrict,
    List<RuleTrigger> Triggers,
    List<RuleAction> Actions,
    string? RuleGroupName
);

public record RunRuleRequest(string StartDate, string EndDate);

public class RuleProfile: Profile {
    public RuleProfile() {
        CreateMap<RuleRequest, Rule>();
    }
}

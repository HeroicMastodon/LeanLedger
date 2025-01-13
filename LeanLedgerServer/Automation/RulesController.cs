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

        var results = await FindMatchingTransactionsForRule(rule, startDate, endDate, limit);

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

        var changedCount = await RunSingleRule(rule, request.StartDate, request.EndDate);

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
            changedCount += await RunSingleRule(
                rule,
                request.StartDate,
                request.EndDate
            );
        }

        return Ok(new {Count = changedCount});
    }

    private async Task<int> RunSingleRule(
        Rule rule,
        string start,
        string end
    ) {
        var (startDate, endDate) = (
            DateOnly.Parse(start, CultureInfo.InvariantCulture),
            DateOnly.Parse(end, CultureInfo.InvariantCulture)
        );

        var transactions = await FindMatchingTransactionsForRule(
            rule,
            startDate,
            endDate
        );
        transactions.ForEach(rule.ApplyActionsTo);
        dbContext.UpdateRange(transactions);
        await dbContext.SaveChangesAsync();

        return transactions.Count;
    }

    private async Task<List<Transaction>> FindMatchingTransactionsForRule(
        Rule rule,
        DateOnly startDate,
        DateOnly endDate,
        int? limit = null
    ) {
        var queryString = "select * from transactions";
        List<SqliteParameter?> values = [];
        foreach (var ((field, not, condition, value), index) in rule.Triggers.Enumerate()) {
            var prefix = index == 0 ? "where" : "and";
            var fieldName = field switch {
                RuleTransactionField.Description => "Description",
                RuleTransactionField.Date => "Date",
                RuleTransactionField.Amount => "Amount",
                RuleTransactionField.Type => "Type",
                RuleTransactionField.Category => "Category",
                RuleTransactionField.Source => "SourceAccountId",
                RuleTransactionField.Destination => "DestinationAccountId",
                _ => throw new UnreachableException()
            };
            var valueName = $"@value{index}";
            var comparison = condition switch {
                RuleCondition.StartsWith => $"{NotToString(not)} like {valueName} + '%'",
                RuleCondition.EndsWith => $"{NotToString(not)} like '%' + {valueName}",
                RuleCondition.Contains => $"{NotToString(not)} like '%' + {valueName} + '%'",
                RuleCondition.IsExactly => $"{NotToString(not)} like {valueName}",
                RuleCondition.GreaterThan => $"{(not ? "<=" : ">")} {valueName}",
                RuleCondition.LessThan => $"{(not ? ">=" : "<")} {valueName}",
                RuleCondition.Exists => $"is {NotToString(!not)} null",
                _ => throw new UnreachableException()
            };

            queryString += $" {prefix} {fieldName} {comparison}";
            values.Add(new SqliteParameter(valueName, value));
        }

        // avoiding some weird covariant error I guess
        var valuesArray = values.Select(object? (v) => v).ToArray();
        var query = dbContext
            .Transactions
            .FromSqlRaw(queryString, valuesArray)
            .Where(t => t.Date >= startDate && t.Date <= endDate);

        if (limit is not null) {
            query = query.Take(limit.Value);
        }

        var results = await query.ToListAsync();

        return results;
    }


    private static string NotToString(bool? not) => not == true ? "not" : "";
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

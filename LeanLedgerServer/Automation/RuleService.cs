namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Globalization;
using Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Transactions;

public class RuleService(LedgerDbContext dbContext) {
    public async Task<int> Run(Rule rule, string start, string end) {
        var (startDate, endDate) = (
            DateOnly.Parse(start, CultureInfo.InvariantCulture),
            DateOnly.Parse(end, CultureInfo.InvariantCulture)
        );

        var transactions = await FindMatchingTransactionsFor(
            rule,
            startDate,
            endDate
        );
        transactions.ForEach(rule.ApplyActionsTo);
        dbContext.UpdateRange(transactions);
        await dbContext.SaveChangesAsync();

        return transactions.Count;
    }

    public async Task<List<Transaction>> FindMatchingTransactionsFor(
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
                RuleCondition.StartsWith => $"{NotToString(not)} like {valueName} || '%'",
                RuleCondition.EndsWith => $"{NotToString(not)} like '%' || {valueName}",
                RuleCondition.Contains => $"{NotToString(not)} like '%' || {valueName} || '%'",
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

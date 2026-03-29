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

        var allocationsToAdd = new List<LeanLedgerServer.PiggyBanks.PiggyAllocation>();

        foreach (var transaction in transactions) {
            var pending = rule.ApplyActionsTo(transaction);

            if (pending is not null && pending.Count > 0) {
                // validate and prepare allocations
                if (transaction.IsDeleted) {
                    throw new InvalidOperationException($"Cannot apply rule allocations to deleted transaction {transaction.Id}");
                }

                if (transaction.Type == TransactionType.Transfer) {
                    throw new InvalidOperationException($"Cannot add allocations to transfer transaction {transaction.Id}");
                }

                var existing = await dbContext.PiggyAllocations.Where(a => a.TransactionId == transaction.Id).SumAsync(a => a.Amount);
                var pendingSum = pending.Sum(p => p.Amount);
                if (existing + pendingSum > transaction.Amount) {
                    throw new InvalidOperationException($"Rule would create allocations exceeding transaction amount for {transaction.Id}");
                }

                foreach (var p in pending) {
                    var piggy = await dbContext.PiggyBanks.FindAsync(p.PiggyBankId);
                    if (piggy is null) throw new InvalidOperationException($"Piggy bank {p.PiggyBankId} not found");
                    if (piggy.Closed) throw new InvalidOperationException($"Piggy bank {p.PiggyBankId} is closed");

                    allocationsToAdd.Add(new LeanLedgerServer.PiggyBanks.PiggyAllocation {
                        Id = Guid.NewGuid(),
                        TransactionId = transaction.Id,
                        PiggyBankId = p.PiggyBankId,
                        Amount = p.Amount
                    });
                }
            }
        }

        dbContext.UpdateRange(transactions);
        if (allocationsToAdd.Count > 0) await dbContext.AddRangeAsync(allocationsToAdd);
        await dbContext.SaveChangesAsync();

        return transactions.Count;
    }

    public async Task<List<Transaction>> FindMatchingTransactionsFor(
        Rule rule,
        DateOnly startDate,
        DateOnly endDate,
        int? limit = null,
        bool includeAccounts = false
    ) {
        var queryString = "select * from transactions";
        List<SqliteParameter?> values = [];
        foreach (var ((field, not, condition, value), index) in rule.Triggers.Enumerate()) {
            var prefix = index == 0 ? "where" : "and";
            var fieldName = field switch {
                RuleTransactionField.Description => "Description",
                RuleTransactionField.Date => "Date",
                RuleTransactionField.Amount => "Cast(Amount as REAL)",
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
            object? finalValue = decimal.TryParse(value, out var parsed) ? parsed : value;
            values.Add(new SqliteParameter(valueName, finalValue));
        }

        // avoiding some weird covariant error I guess
        var valuesArray = values.Select(object? (v) => v).ToArray();
        var query = dbContext
            .Transactions
            .FromSqlRaw(queryString, valuesArray)
            .Where(t => t.Date >= startDate && t.Date <= endDate);

        if (includeAccounts) {
            query = query
                .Include(t => t.DestinationAccount)
                .Include(t => t.SourceAccount);
        }

        if (limit is not null) {
            query = query.Take(limit.Value);
        }

        var results = await query.ToListAsync();

        return results;
    }


    private static string NotToString(bool? not) => not == true ? "not" : "";
}

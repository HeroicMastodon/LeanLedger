namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Common;
using Transactions;

public class Rule {
    public required Guid Id { get; init; }
    public string Name { get; set; }
    public bool IsStrict { get; set; }
    public List<RuleTrigger> Triggers { get; set; }
    public List<RuleAction> Actions { get; set; }
    public bool IsDeleted { get; set; }

    public string? RuleGroupName { get; set; }
    [JsonIgnore]
    public RuleGroup? RuleGroup { get; set; }

    public bool ShouldTriggerFor(Transaction transaction) => IsStrict
        ? Triggers.All(t => t.Matches(transaction))
        : Triggers.Any(t => t.Matches(transaction));

    public void ApplyActionsTo(Transaction transaction) => Actions.ForEach(a => a.ApplyTo(transaction));
}

public class RuleGroup {
    public required string Name { get; set; }
    public List<Rule> Rules { get; set; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleTransactionField {
    Description,
    Date,
    Amount,
    Type,
    Category,
    Source,
    Destination
}

public static class TransactionRuleFieldFunctions {
    public static string? GetValueFrom(this RuleTransactionField field, Transaction transaction, string? dateFormat = null) => field switch {
        RuleTransactionField.Description => transaction.Description,
        RuleTransactionField.Date => transaction.Date.ToString(dateFormat ?? "yyyy-MM-dd", CultureInfo.InvariantCulture),
        RuleTransactionField.Amount => transaction.Amount.ToString(CultureInfo.InvariantCulture),
        RuleTransactionField.Type => transaction.Type.ToString(),
        RuleTransactionField.Category => transaction.Category,
        RuleTransactionField.Source => transaction.SourceAccountId.ToString(),
        RuleTransactionField.Destination => transaction.DestinationAccountId.ToString(),
        _ => throw new UnreachableException()
    };

    public static void ApplyValueTo(this RuleTransactionField field, Transaction transaction, string? value) {
        switch (field) {
            case RuleTransactionField.Description:
                transaction.Description = value;
                break;
            case RuleTransactionField.Date:
                if (value is null) {
                    throw new InvalidOperationException();
                }

                transaction.Date = DateOnly.Parse(value, CultureInfo.InvariantCulture);
                break;
            case RuleTransactionField.Amount:
                if (value is null) {
                    throw new InvalidOperationException();
                }

                transaction.Amount = decimal.Parse(value, CultureInfo.InvariantCulture);
                break;
            case RuleTransactionField.Type:
                if (value is null) {
                    throw new InvalidOperationException();
                }

                transaction.Type = Enum.Parse<TransactionType>(value ?? "");
                break;
            case RuleTransactionField.Category:
                transaction.Category = value;
                break;
            case RuleTransactionField.Source:
                transaction.SourceAccountId = value is not null ? Guid.Parse(value) : null;
                break;
            case RuleTransactionField.Destination:
                transaction.DestinationAccountId = value is not null ? Guid.Parse(value) : null;
                break;
            default:
                throw new UnreachableException();
        }
    }
}

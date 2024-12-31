namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Transactions;

public class Rule {
    public Guid Id { get; init; }
    public string Name { get; set; }
    public bool IsStrict { get; set; }
    public List<RuleTrigger> Triggers { get; set; }
    public List<RuleAction> Actions { get; set; }
    public bool IsDeleted { get; set; }

    public string? RuleGroupName { get; set; }
    public RuleGroup? RuleGroup { get; set; }

    public bool ShouldTriggerFor(Transaction transaction) => IsStrict
        ? Triggers.All(t => t.Matches(transaction))
        : Triggers.Any(t => t.Matches(transaction));

    public void ApplyActionsTo(Transaction transaction) => Actions.ForEach(a => a.ApplyTo(transaction));
}

public class RuleGroup {
    public string Name { get; set; }
    public List<Rule> Rules { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionRuleField {
    Description,
    Date,
    Amount,
    Type,
    Category,
    Source,
    Destination
}

public static class TransactionRuleFieldFunctions {
    public static string? GetValueFrom(this TransactionRuleField field, Transaction transaction, string? dateFormat = null) => field switch {
        TransactionRuleField.Description => transaction.Description,
        TransactionRuleField.Date => transaction.Date.ToString(dateFormat ?? "yyyy-MM-dd", CultureInfo.InvariantCulture),
        TransactionRuleField.Amount => transaction.Amount.ToString(CultureInfo.InvariantCulture),
        TransactionRuleField.Type => transaction.Type.ToString(),
        TransactionRuleField.Category => transaction.Category,
        TransactionRuleField.Source => transaction.SourceAccountId.ToString(),
        TransactionRuleField.Destination => transaction.DestinationAccountId.ToString(),
        _ => throw new UnreachableException()
    };

    public static void ApplyValueTo(this TransactionRuleField field, Transaction transaction, string? value) {
        switch (field) {
            case TransactionRuleField.Description:
                transaction.Description = value;
                break;
            case TransactionRuleField.Date:
                if (value is null) {
                    throw new InvalidOperationException();
                }

                transaction.Date = DateOnly.Parse(value, CultureInfo.InvariantCulture);
                break;
            case TransactionRuleField.Amount:
                if (value is null) {
                    throw new InvalidOperationException();
                }

                transaction.Amount = decimal.Parse(value, CultureInfo.InvariantCulture);
                break;
            case TransactionRuleField.Type:
                if (value is null) {
                    throw new InvalidOperationException();
                }

                transaction.Type = Enum.Parse<TransactionType>(value ?? "");
                break;
            case TransactionRuleField.Category:
                transaction.Category = value;
                break;
            case TransactionRuleField.Source:
                transaction.SourceAccountId = value is not null ? Guid.Parse(value) : null;
                break;
            case TransactionRuleField.Destination:
                transaction.DestinationAccountId = value is not null ? Guid.Parse(value) : null;
                break;
            default:
                throw new UnreachableException();
        }
    }
}

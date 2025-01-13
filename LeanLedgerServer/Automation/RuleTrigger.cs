namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Text.Json.Serialization;
using Transactions;

public record RuleTrigger(
    RuleTransactionField Field,
    bool Not,
    RuleCondition Condition,
    string? Value
) {
    public bool Matches(Transaction transaction) {
        var value = Field.GetValueFrom(transaction);
        var comparisonValue = Value ?? "";
        var result = value is not null && Condition switch {
            RuleCondition.StartsWith => value.StartsWith(comparisonValue, StringComparison.InvariantCultureIgnoreCase),
            RuleCondition.EndsWith => value.EndsWith(comparisonValue, StringComparison.InvariantCultureIgnoreCase),
            RuleCondition.Contains => value.Contains(comparisonValue, StringComparison.InvariantCultureIgnoreCase),
            RuleCondition.IsExactly => value == comparisonValue,
            RuleCondition.GreaterThan => string.CompareOrdinal(value, comparisonValue) > 0,
            RuleCondition.LessThan => string.CompareOrdinal(value, comparisonValue) < 0,
            RuleCondition.Exists => !string.IsNullOrWhiteSpace(value),
            _ => throw new UnreachableException()
        };

        return Not ? !result : result;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleCondition {
    StartsWith,
    EndsWith,
    Contains,
    IsExactly,
    GreaterThan,
    LessThan,
    Exists,
}

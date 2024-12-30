namespace LeanLedgerServer.Automation;

using System.Text.Json.Serialization;
using Transactions;

public record RuleTrigger(
    TransactionRuleField Field,
    bool Not,
    RuleCondition Condition,
    string? Value
) {
    public bool Matches(Transaction transaction) => false;
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

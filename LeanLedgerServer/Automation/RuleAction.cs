namespace LeanLedgerServer.Automation;

using System.Text.Json.Serialization;
using Transactions;

public record RuleAction(
    RuleActionType ActionType,
    TransactionRuleField? Field,
    string? Value
) {
    public void ApplyTo(Transaction transaction) { }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleActionType {
    Append,
    Set,
    Clear,
    DeleteTransaction
}

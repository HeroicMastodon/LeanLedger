namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Text.Json.Serialization;
using Transactions;

public record RuleAction(
    RuleActionType ActionType,
    RuleTransactionField? Field,
    string? Value
) {
    public void ApplyTo(Transaction transaction) {
        switch (ActionType) {
            case RuleActionType.Append:
                Append(transaction);
                break;
            case RuleActionType.Set:
                Set(transaction);
                break;
            case RuleActionType.Clear:
                Clear(transaction);
                break;
            case RuleActionType.DeleteTransaction:
                Delete(transaction);
                break;
            default:
                throw new UnreachableException();
        }
    }

    private void Append(Transaction transaction) {
        if (Field is null) { throw new InvalidOperationException(); }

        var transactionValue = Field.Value.GetValueFrom(transaction);

        if (transactionValue is null) {
            Field.Value.ApplyValueTo(transaction, Value);
            return;
        }

        transactionValue = transactionValue + Value;
        Field.Value.ApplyValueTo(transaction, transactionValue);
    }

    private void Set(Transaction transaction) {
        if (Field is null) { throw new InvalidOperationException(); }
        Field.Value.ApplyValueTo(transaction, Value);
    }

    private void Clear(Transaction transaction) {
        if (Field is null) { throw new InvalidOperationException(); }
        Field.Value.ApplyValueTo(transaction, null);
    }

    private void Delete(Transaction transaction) => transaction.IsDeleted = true;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleActionType {
    Append,
    Set,
    Clear,
    DeleteTransaction
}

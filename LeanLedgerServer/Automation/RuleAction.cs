namespace LeanLedgerServer.Automation;

using System.Diagnostics;
using System.Text.Json.Serialization;
using Transactions;

public record RuleAction(
    RuleActionType ActionType,
    RuleTransactionField? Field,
    string? Value,
    Guid? PiggyBankId = null,
    decimal? PiggyAmount = null
) {
    public void ApplyTo(Transaction transaction) {
        ApplyTo(transaction, null);
    }

    public void ApplyTo(Transaction transaction, List<LeanLedgerServer.Transactions.PendingAllocation>? pendingAllocations) {
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
            case RuleActionType.AddPiggyAllocation:
                AddPiggyAllocation(pendingAllocations);
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

    private void AddPiggyAllocation(List<LeanLedgerServer.Transactions.PendingAllocation>? pendingAllocations) {
        if (pendingAllocations is null) {
            // nothing to do when not in creation flow
            return;
        }

        // Prefer explicit properties when present (new rules)
        if (PiggyBankId is not null && PiggyAmount is not null) {
            pendingAllocations.Add(new LeanLedgerServer.Transactions.PendingAllocation(PiggyBankId.Value, PiggyAmount.Value));
            return;
        }

        if (Field is null || Value is null) { throw new InvalidOperationException(); }

        // Fallback to legacy storage: Field contains piggy id, Value contains amount
        if (!Guid.TryParse(Field.Value.ToString(), out var piggyId)) {
            throw new InvalidOperationException();
        }

        if (!decimal.TryParse(Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount)) {
            throw new InvalidOperationException();
        }

        pendingAllocations.Add(new LeanLedgerServer.Transactions.PendingAllocation(piggyId, amount));
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RuleActionType {
        Append,
        Set,
        Clear,
        DeleteTransaction
        ,
        AddPiggyAllocation
    }

namespace Tests.Automation.RuleAction;

using System.Diagnostics;
using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class ApplyTo {
    [TestCase(
        nameof(Append),
        RuleTransactionField.Description,
        "starting",
        "new value",
        "startingnew value"
    )]
    [TestCase(
        nameof(Append),
        RuleTransactionField.Description,
        null,
        "new value",
        "new value"
    )]
    [TestCase(
        nameof(Set),
        RuleTransactionField.Description,
        "starting",
        "new value",
        "new value"
    )]
    [TestCase(
        nameof(Clear),
        RuleTransactionField.Description,
        "starting",
        null,
        null
    )]
    public void GivenAnActionTypeFieldAndValue_SetTransactionFieldAppropriately(
        string actionType,
        RuleTransactionField field,
        string? startingValue,
        string? actionValue,
        string? expectedValue
    ) {
        var action = CreateAction(actionType, field, actionValue);
        var transaction = new Transaction {
            UniqueHash = "",
            Id = Guid.NewGuid()
        };

        field.ApplyValueTo(transaction, startingValue);

        RunAndAssert(action, transaction, expectedValue);
    }

    [Test]
    public void WhenActionIsDeleteTransaction_TransactionIsDeletedIsTrue() {
        var action = CreateAction(nameof(DeleteTransaction));
        var transaction = new Transaction {
            UniqueHash = "",
            Id = Guid.NewGuid()
        };

        action.RunOn(transaction);

        Assert.That(transaction.IsDeleted, Is.True);
    }

    private RuleAction CreateAction(string actionType, RuleTransactionField? field = null, string? value = null)
        => actionType switch {
            "Append" => new Append(field!.Value, value!),
            "Set" => new Set(field!.Value, value!),
            "Clear" => new Clear(field!.Value),
            "DeleteTransaction" => new DeleteTransaction(),
            _ => throw new UnreachableException()
        };

    private void RunAndAssert(RuleAction action, Transaction transaction, string? expectedValue) {
        action.RunOn(transaction);
        var result = action switch {
            Append append => append.Field.GetValueFrom(transaction),
            Set set => set.Field.GetValueFrom(transaction),
            Clear clear => clear.Field.GetValueFrom(transaction),
            DeleteTransaction => null,
            _ => throw new UnreachableException()
        };

        Assert.That(result, Is.EqualTo(expectedValue));
    }
}

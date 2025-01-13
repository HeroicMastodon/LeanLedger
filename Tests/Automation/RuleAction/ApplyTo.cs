namespace Tests.Automation.RuleAction;

using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class ApplyTo {
    [TestCase(
        RuleActionType.Append,
        RuleTransactionField.Description,
        "starting",
        "new value",
        "startingnew value"
    )]
    [TestCase(
        RuleActionType.Append,
        RuleTransactionField.Description,
        null,
        "new value",
        "new value"
    )]
    [TestCase(
        RuleActionType.Set,
        RuleTransactionField.Description,
        "starting",
        "new value",
        "new value"
    )]
    [TestCase(
        RuleActionType.Clear,
        RuleTransactionField.Description,
        "starting",
        null,
        null
    )]
    [TestCase(
        RuleActionType.Clear,
        RuleTransactionField.Description,
        "starting",
        "unused",
        null
    )]
    public void GivenAnActionTypeFieldAndValue_SetTransactionFieldAppropriately(
        RuleActionType actionType,
        RuleTransactionField field,
        string? startingValue,
        string? actionValue,
        string? expectedValue
    ) {
        var action = new RuleAction(actionType, field, actionValue);
        var transaction = new Transaction {
            UniqueHash = "",
            Id = Guid.NewGuid()
        };

        field.ApplyValueTo(transaction, startingValue);

        action.ApplyTo(transaction);
        var result = field.GetValueFrom(transaction);

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public void WhenActionIsDeleteTransaction_TransactionIsDeletedIsTrue() {
         var action = new RuleAction(RuleActionType.DeleteTransaction, null, null);
         var transaction = new Transaction {
             UniqueHash = "",
             Id = Guid.NewGuid()
         };

         action.ApplyTo(transaction);

         Assert.That(transaction.IsDeleted, Is.True);
    }
}

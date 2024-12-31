namespace Tests.Automation.RuleTransactionField;

using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class GetValueFrom {
    [TestCase(TransactionRuleField.Description, "A description")]
    [TestCase(TransactionRuleField.Date, "2024-01-11")]
    [TestCase(TransactionRuleField.Amount, "1234")]
    [TestCase(TransactionRuleField.Type, "Expense")]
    [TestCase(TransactionRuleField.Category, "A Category")]
    [TestCase(TransactionRuleField.Source, "2836555c-9173-491b-add6-1956e406870d")]
    [TestCase(TransactionRuleField.Destination, "1ff332c8-284b-437b-94ac-deb4f966e143")]
    public void GivenValue_ThenReturnsMatchingValue(
        TransactionRuleField field,
        string value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        field.ApplyValueTo(transaction, value);
        var result = field.GetValueFrom(transaction);

        Assert.That(result, Is.EqualTo(value));
    }
}

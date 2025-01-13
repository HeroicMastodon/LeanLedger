namespace Tests.Automation.RuleTransactionField;

using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class GetValueFrom {
    [TestCase(RuleTransactionField.Description, "A description")]
    [TestCase(RuleTransactionField.Date, "2024-01-11")]
    [TestCase(RuleTransactionField.Amount, "1234")]
    [TestCase(RuleTransactionField.Type, "Expense")]
    [TestCase(RuleTransactionField.Category, "A Category")]
    [TestCase(RuleTransactionField.Source, "2836555c-9173-491b-add6-1956e406870d")]
    [TestCase(RuleTransactionField.Destination, "1ff332c8-284b-437b-94ac-deb4f966e143")]
    public void GivenValue_ThenReturnsMatchingValue(
        RuleTransactionField field,
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

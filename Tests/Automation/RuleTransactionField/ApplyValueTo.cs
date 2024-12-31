namespace Tests.Automation.RuleTransactionField;

using System.Globalization;
using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class ApplyValueTo {
    [TestCase("A description")]
    [TestCase("A category")]
    public void GivenStringValue_WhenDescription_ThenTransactionDescriptionMatchesValue(
        string value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Description.ApplyValueTo(transaction, value);

        Assert.That(transaction.Description, Is.EqualTo(value));
    }

    [TestCase("A description")]
    [TestCase("A category")]
    public void GivenStringValue_WhenCategory_ThenTransactionCategoryMatchesValue(
        string value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Category.ApplyValueTo(transaction, value);

        Assert.That(transaction.Category, Is.EqualTo(value));
    }

    [TestCase(1234)]
    [TestCase(0)]
    public void GivenNumberValue_WhenAmount_ThenTransactionAmountMatchesValue(
        decimal value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Amount.ApplyValueTo(transaction, value.ToString(CultureInfo.InvariantCulture));

        Assert.That(transaction.Amount, Is.EqualTo(value));
    }

    [TestCase(TransactionType.Transfer)]
    [TestCase(TransactionType.Income)]
    public void GivenTransactionTypeValue_WhenTransactionType_ThenTransactionTypeMatchesValue(
        TransactionType value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Type.ApplyValueTo(transaction, value.ToString());

        Assert.That(transaction.Type, Is.EqualTo(value));
    }


    [TestCase("2024-01-02")]
    [TestCase("2026-10-12")]
    public void GivenDateValue_WhenDate_ThenTransactionDateMatchesValue(
        string value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Date.ApplyValueTo(transaction, value);

        Assert.That(transaction.Date, Is.EqualTo(DateOnly.Parse(value)));
    }

    [TestCase("2836555c-9173-491b-add6-1956e406870d")]
    [TestCase("1ff332c8-284b-437b-94ac-deb4f966e143")]
    public void GivenSourceValue_WhenSource_ThenTransactionSourceMatchesValue(
        string value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Source.ApplyValueTo(transaction, value);

        Assert.That(transaction.SourceAccountId, Is.EqualTo(Guid.Parse(value)));
    }

    [TestCase("2836555c-9173-491b-add6-1956e406870d")]
    [TestCase("1ff332c8-284b-437b-94ac-deb4f966e143")]
    public void GivenDestinationValue_WhenDestination_ThenTransactionDestinationMatchesValue(
        string value
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        TransactionRuleField.Destination.ApplyValueTo(transaction, value);

        Assert.That(transaction.DestinationAccountId, Is.EqualTo(Guid.Parse(value)));
    }
}

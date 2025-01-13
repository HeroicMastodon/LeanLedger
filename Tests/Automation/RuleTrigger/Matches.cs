namespace Tests.Automation.RuleTrigger;

using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class Matches {
    [TestCase(RuleTransactionField.Description, RuleCondition.StartsWith, "a description", "a desc")]
    [TestCase(RuleTransactionField.Category, RuleCondition.StartsWith, "a category", "a cat")]
    public void GivenNotIsTrue_ThenResultIsNegated(
        RuleTransactionField field,
        RuleCondition condition,
        string value,
        string prefix
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
            Description = value,
            Category = value
        };
        var trigger = new RuleTrigger(
            field,
            Not: true,
            condition,
            prefix
        );
        var result = trigger.Matches(transaction);

        Assert.That(result, Is.False);
    }

    [TestCase(RuleTransactionField.Description, RuleCondition.StartsWith, "a description", "a desc")]
    [TestCase(RuleTransactionField.Category, RuleCondition.StartsWith, "a category", "a cat")]
    [TestCase(RuleTransactionField.Description, RuleCondition.EndsWith, "a description", "tion")]
    [TestCase(RuleTransactionField.Category, RuleCondition.EndsWith, "a category", "ory")]
    [TestCase(RuleTransactionField.Description, RuleCondition.Contains, "a description", "escr")]
    [TestCase(RuleTransactionField.Category, RuleCondition.Contains, "a category", "atego")]
    [TestCase(RuleTransactionField.Description, RuleCondition.IsExactly, "a description", "a description")]
    [TestCase(RuleTransactionField.Category, RuleCondition.IsExactly, "a category", "a category")]
    [TestCase(RuleTransactionField.Date, RuleCondition.IsExactly, "2024-10-01", "2024-10-01")]
    [TestCase(RuleTransactionField.Amount, RuleCondition.IsExactly, "1234", "1234")]
    [TestCase(RuleTransactionField.Type, RuleCondition.IsExactly, "Expense", "Expense")]
    [TestCase(RuleTransactionField.Source, RuleCondition.IsExactly, "2836555c-9173-491b-add6-1956e406870d", "2836555c-9173-491b-add6-1956e406870d")]
    [TestCase(RuleTransactionField.Destination, RuleCondition.IsExactly, "1ff332c8-284b-437b-94ac-deb4f966e143", "1ff332c8-284b-437b-94ac-deb4f966e143")]
    [TestCase(RuleTransactionField.Description, RuleCondition.Exists, "a description", "a desc")]
    [TestCase(RuleTransactionField.Category, RuleCondition.Exists, "a category", "a cat")]
    [TestCase(RuleTransactionField.Date, RuleCondition.GreaterThan, "2024-10-01", "2024-09-01")]
    [TestCase(RuleTransactionField.Amount, RuleCondition.GreaterThan, "1234", "1232")]
    [TestCase(RuleTransactionField.Date, RuleCondition.LessThan, "2024-10-01", "2024-11-01")]
    [TestCase(RuleTransactionField.Amount, RuleCondition.LessThan, "1234", "1239")]
    public void GivenAStringField_WhenConditionIsMet_ThenReturnTrue(
        RuleTransactionField field,
        RuleCondition condition,
        string value,
        string prefix
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        field.ApplyValueTo(transaction, value);

        var trigger = new RuleTrigger(
            field,
            Not: false,
            condition,
            prefix
        );
        var result = trigger.Matches(transaction);

        Assert.That(result, Is.True);
    }

    [TestCase(RuleTransactionField.Description, RuleCondition.StartsWith, "a description", "desc")]
    [TestCase(RuleTransactionField.Category, RuleCondition.StartsWith, "a category", "cat")]
    [TestCase(RuleTransactionField.Description, RuleCondition.EndsWith, "a description", "descriptio")]
    [TestCase(RuleTransactionField.Category, RuleCondition.EndsWith, "a category", "cate")]
    [TestCase(RuleTransactionField.Description, RuleCondition.Contains, "a description", "hello")]
    [TestCase(RuleTransactionField.Category, RuleCondition.Contains, "a category", "hello")]
    [TestCase(RuleTransactionField.Description, RuleCondition.IsExactly, "a description", "a descriptiion")]
    [TestCase(RuleTransactionField.Category, RuleCondition.IsExactly, "a category", "a cateegory")]
    [TestCase(RuleTransactionField.Date, RuleCondition.IsExactly, "2024-10-01", "2024-10-02")]
    [TestCase(RuleTransactionField.Amount, RuleCondition.IsExactly, "1234", "1324")]
    [TestCase(RuleTransactionField.Type, RuleCondition.IsExactly, "Expense", "Expnse")]
    [TestCase(RuleTransactionField.Source, RuleCondition.IsExactly, "2836555c-9172-491b-add6-1956e406870d", "2836555c-9173-491b-add6-1956e406870d")]
    [TestCase(RuleTransactionField.Destination, RuleCondition.IsExactly, "1ff332c7-284b-437b-94ac-deb4f966e143", "1ff332c8-284b-437b-94ac-deb4f966e143")]
    [TestCase(RuleTransactionField.Date, RuleCondition.GreaterThan, "2024-10-01", "2024-10-01")]
    [TestCase(RuleTransactionField.Amount, RuleCondition.GreaterThan, "1234", "1236")]
    [TestCase(RuleTransactionField.Date, RuleCondition.LessThan, "2024-10-01", "2024-09-01")]
    [TestCase(RuleTransactionField.Amount, RuleCondition.LessThan, "1234", "1234")]
    [TestCase(RuleTransactionField.Description, RuleCondition.Exists, null, null)]
    [TestCase(RuleTransactionField.Category, RuleCondition.Exists, null, null)]
    public void GivenAStringField_WhenConditionIsNotMet_ThenReturnFalse(
        RuleTransactionField field,
        RuleCondition condition,
        string? value,
        string? prefix
    ) {
        var transaction = new Transaction {
            Id = Guid.NewGuid(),
            UniqueHash = "",
        };
        field.ApplyValueTo(transaction, value);
        var trigger = new RuleTrigger(
            field,
            Not: false,
            condition,
            prefix
        );
        var result = trigger.Matches(transaction);

        Assert.That(result, Is.False);
    }
}

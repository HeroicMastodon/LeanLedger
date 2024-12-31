namespace Tests.Automation.RuleTrigger;

using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;

public class Matches {
    [TestCase(TransactionRuleField.Description, RuleCondition.StartsWith, "a description", "a desc")]
    [TestCase(TransactionRuleField.Category, RuleCondition.StartsWith, "a category", "a cat")]
    public void GivenNotIsTrue_ThenResultIsNegated(
        TransactionRuleField field,
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

    [TestCase(TransactionRuleField.Description, RuleCondition.StartsWith, "a description", "a desc")]
    [TestCase(TransactionRuleField.Category, RuleCondition.StartsWith, "a category", "a cat")]
    [TestCase(TransactionRuleField.Description, RuleCondition.EndsWith, "a description", "tion")]
    [TestCase(TransactionRuleField.Category, RuleCondition.EndsWith, "a category", "ory")]
    [TestCase(TransactionRuleField.Description, RuleCondition.Contains, "a description", "escr")]
    [TestCase(TransactionRuleField.Category, RuleCondition.Contains, "a category", "atego")]
    [TestCase(TransactionRuleField.Description, RuleCondition.IsExactly, "a description", "a description")]
    [TestCase(TransactionRuleField.Category, RuleCondition.IsExactly, "a category", "a category")]
    [TestCase(TransactionRuleField.Date, RuleCondition.IsExactly, "2024-10-01", "2024-10-01")]
    [TestCase(TransactionRuleField.Amount, RuleCondition.IsExactly, "1234", "1234")]
    [TestCase(TransactionRuleField.Type, RuleCondition.IsExactly, "Expense", "Expense")]
    [TestCase(TransactionRuleField.Source, RuleCondition.IsExactly, "2836555c-9173-491b-add6-1956e406870d", "2836555c-9173-491b-add6-1956e406870d")]
    [TestCase(TransactionRuleField.Destination, RuleCondition.IsExactly, "1ff332c8-284b-437b-94ac-deb4f966e143", "1ff332c8-284b-437b-94ac-deb4f966e143")]
    [TestCase(TransactionRuleField.Description, RuleCondition.Exists, "a description", "a desc")]
    [TestCase(TransactionRuleField.Category, RuleCondition.Exists, "a category", "a cat")]
    [TestCase(TransactionRuleField.Date, RuleCondition.GreaterThan, "2024-10-01", "2024-09-01")]
    [TestCase(TransactionRuleField.Amount, RuleCondition.GreaterThan, "1234", "1232")]
    [TestCase(TransactionRuleField.Date, RuleCondition.LessThan, "2024-10-01", "2024-11-01")]
    [TestCase(TransactionRuleField.Amount, RuleCondition.LessThan, "1234", "1239")]
    public void GivenAStringField_WhenConditionIsMet_ThenReturnTrue(
        TransactionRuleField field,
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

    [TestCase(TransactionRuleField.Description, RuleCondition.StartsWith, "a description", "desc")]
    [TestCase(TransactionRuleField.Category, RuleCondition.StartsWith, "a category", "cat")]
    [TestCase(TransactionRuleField.Description, RuleCondition.EndsWith, "a description", "descriptio")]
    [TestCase(TransactionRuleField.Category, RuleCondition.EndsWith, "a category", "cate")]
    [TestCase(TransactionRuleField.Description, RuleCondition.Contains, "a description", "hello")]
    [TestCase(TransactionRuleField.Category, RuleCondition.Contains, "a category", "hello")]
    [TestCase(TransactionRuleField.Description, RuleCondition.IsExactly, "a description", "a descriptiion")]
    [TestCase(TransactionRuleField.Category, RuleCondition.IsExactly, "a category", "a cateegory")]
    [TestCase(TransactionRuleField.Date, RuleCondition.IsExactly, "2024-10-01", "2024-10-02")]
    [TestCase(TransactionRuleField.Amount, RuleCondition.IsExactly, "1234", "1324")]
    [TestCase(TransactionRuleField.Type, RuleCondition.IsExactly, "Expense", "Expnse")]
    [TestCase(TransactionRuleField.Source, RuleCondition.IsExactly, "2836555c-9172-491b-add6-1956e406870d", "2836555c-9173-491b-add6-1956e406870d")]
    [TestCase(TransactionRuleField.Destination, RuleCondition.IsExactly, "1ff332c7-284b-437b-94ac-deb4f966e143", "1ff332c8-284b-437b-94ac-deb4f966e143")]
    [TestCase(TransactionRuleField.Date, RuleCondition.GreaterThan, "2024-10-01", "2024-10-01")]
    [TestCase(TransactionRuleField.Amount, RuleCondition.GreaterThan, "1234", "1236")]
    [TestCase(TransactionRuleField.Date, RuleCondition.LessThan, "2024-10-01", "2024-09-01")]
    [TestCase(TransactionRuleField.Amount, RuleCondition.LessThan, "1234", "1234")]
    [TestCase(TransactionRuleField.Description, RuleCondition.Exists, null, null)]
    [TestCase(TransactionRuleField.Category, RuleCondition.Exists, null, null)]
    public void GivenAStringField_WhenConditionIsNotMet_ThenReturnFalse(
        TransactionRuleField field,
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

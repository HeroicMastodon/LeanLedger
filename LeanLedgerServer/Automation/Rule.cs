namespace LeanLedgerServer.Automation;

using System.Text.Json.Serialization;

public class Rule {
    public Guid Id { get; init; }
    public string Name { get; set; }
    public bool IsStrict { get; set; }
    public List<RuleTrigger> Triggers { get; set; }
    public List<RuleAction> Actions { get; set; }
    public bool IsDeleted { get; set; }

    public string? RuleGroupName { get; set; }
    public RuleGroup? RuleGroup { get; set; }
}

public class RuleGroup {
    public string Name { get; set; }
    public List<Rule> Rules { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionRuleField {
    Description,
    Date,
    Amount,
    Type,
    Category,
    Source,
    Destination,
}


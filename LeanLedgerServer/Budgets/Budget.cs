namespace LeanLedgerServer.Budgets;

using System.Collections.Immutable;

public class Budget {
    public required Guid Id { get; init; }
    public required int Month { get; init; }
    public required int Year { get; init; }
    public required decimal ExpectedIncome { get; init; }

    public List<BudgetCategoryGroup> CategoryGroups { get; set; } = [];
}

public record BudgetCategory(string Category, decimal Limit);
public record BudgetCategoryGroup(string Name, ImmutableList<BudgetCategory> Categories);

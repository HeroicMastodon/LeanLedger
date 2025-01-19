namespace LeanLedgerServer.Budgets;

public class Budget {
    public required Guid Id { get; init; }
    public required int Month { get; init; }
    public required int Year { get; init; }
    public required decimal ExpectedIncome { get; init; }

    public List<BudgetCategory> Categories { get; set; } = [];
}

public record BudgetCategory(string Category, decimal Limit);

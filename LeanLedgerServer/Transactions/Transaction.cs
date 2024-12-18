namespace LeanLedgerServer.Transactions;

using System.Text.Json.Serialization;
using Accounts;

public class Transaction {
    public Guid Id { get; init; }
    public string Description { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionType Type { get; set; }
    public required string UniqueHash { get; init; }
    public string? Category { get; set; }
    public bool IsDeleted { get; set; }

    public Guid? SourceAccountId { get; set; }
    [JsonIgnore]
    public Account? SourceAccount { get; set; }
    public Guid? DestinationAccountId { get; set; }
    [JsonIgnore]
    public Account? DestinationAccount { get; set; }
}

public enum TransactionType {
    Expense,
    Income,
    Transfer,
}

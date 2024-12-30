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
    public DateOnly? DateImported { get; init; }

    public Guid? SourceAccountId { get; set; }

    [JsonIgnore]
    public Account? SourceAccount { get; set; }

    public Guid? DestinationAccountId { get; set; }

    [JsonIgnore]
    public Account? DestinationAccount { get; set; }

    public override string ToString() => $"{Type}, {Description}, {Date}, {Amount}, {Category ?? "(none)"}";
}

public enum TransactionType {
    Expense,
    Income,
    Transfer,
}

public record AttachedAccount(
    Guid Id,
    string Name
) {
    public static AttachedAccount? FromAccount(Account? account) => account != null
        ? new AttachedAccount(account.Id, account.Name)
        : null;
}

public record TableTransaction(
    Guid Id,
    string Description,
    decimal Amount,
    DateOnly Date,
    string? Category,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    TransactionType Type,
    AttachedAccount? SourceAccount,
    AttachedAccount? DestinationAccount
) {
    public static TableTransaction FromTransaction(Transaction transaction) => new(
        transaction.Id,
        transaction.Description,
        transaction.Amount,
        transaction.Date,
        transaction.Category,
        transaction.Type,
        AttachedAccount.FromAccount(transaction.SourceAccount),
        AttachedAccount.FromAccount(transaction.DestinationAccount)
    );
}

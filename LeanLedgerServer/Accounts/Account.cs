namespace LeanLedgerServer.Accounts;

using System.Text.Json.Serialization;
using TransactionImport;
using Transactions;

public class Account
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccountType AccountType { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateOnly OpeningDate { get; set; }
    public bool Active { get; set; }
    public bool IncludeInNetWorth { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public List<Transaction> Withdrawls { get; set; } = [];
    public List<Transaction> Deposits { get; set; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    Bank,
    CreditCard,
    Merchant
}

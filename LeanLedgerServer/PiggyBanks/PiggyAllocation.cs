namespace LeanLedgerServer.PiggyBanks;

using System.Text.Json.Serialization;
using Transactions;

public class PiggyAllocation
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }

    [JsonIgnore]
    public Transaction? Transaction { get; set; }

    public Guid PiggyBankId { get; set; }

    [JsonIgnore]
    public PiggyBank? PiggyBank { get; set; }

    public decimal Amount { get; set; }
}

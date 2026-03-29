namespace LeanLedgerServer.PiggyBanks;

using System.Text.Json.Serialization;
using Transactions;

public class PiggyBank
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; } = 0m;
    public decimal? BalanceTarget { get; set; }
    public bool Closed { get; set; } = false;

    [JsonIgnore]
    public List<PiggyAllocation> Allocations { get; set; } = new();
}

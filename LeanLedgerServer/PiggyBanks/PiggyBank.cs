namespace LeanLedgerServer.PiggyBanks;

using LeanLedgerServer.Transactions;

public class PiggyBank {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateOnly OpenDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? CloseDate { get; set; }
    public decimal? TargetBalance { get; set; }
    public bool Closed => CloseDate is not null;

    public List<PiggyBankEntry> Entries { get; set; } = [];
}

public class PiggyBankEntry {
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public Guid PiggyBankId { get; set; }
    public PiggyBank? PiggyBank { get; set; }


    public Guid? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
}

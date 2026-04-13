namespace LeanLedgerServer.PiggyBanks;

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
    public Guid Id { get; set; } = Guid.NewGuid();
    public required DateOnly Date { get; set; }
    public required decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public required Guid PiggyBankId { get; set; }
    public PiggyBank? PiggyBank { get; set; }
}

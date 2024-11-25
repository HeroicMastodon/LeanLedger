namespace LeanLedgerServer.Accounts;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateOnly OpeningDate { get; set; }
    public bool Active { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public enum AccountType
{
    Bank,
    CreditCard
}

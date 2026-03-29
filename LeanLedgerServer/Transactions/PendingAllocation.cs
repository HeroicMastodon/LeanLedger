namespace LeanLedgerServer.Transactions;

public record PendingAllocation(Guid PiggyBankId, decimal Amount);

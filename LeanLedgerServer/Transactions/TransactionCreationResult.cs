namespace LeanLedgerServer.Transactions;

public record TransactionCreationResult(Transaction Transaction, List<PendingAllocation> PendingAllocations);

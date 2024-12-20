namespace LeanLedgerServer.Transactions;

using System.Diagnostics;
using AutoMapper;
using Common;
using Microsoft.EntityFrameworkCore;

public static class TransactionFunctions {
    public static string HashTransactionRequest(TransactionRequest request) => request.GetHashCode().ToString();

    public static async Task<(Transaction?, Err?)> CreateNewTransaction(
        TransactionRequest request,
        IQueryable<Transaction> transactions,
        IMapper mapper
    ) {
        var (transactionType, err) = ValidateTransaction(request);

        if (err is not null) {
            return (null, err);
        }

        if (transactionType is null) {
            throw new UnreachableException("Transaction type cannot be null if err is null");
        }

        var transactionHash = HashTransactionRequest(request);

        // ? In the future it might be a good idea to still check for a conflicting hash and add a (1) or something to it
        if (!request.SkipHashCheck) {
            var existingHash = await transactions.FirstOrDefaultAsync(t => t.UniqueHash == transactionHash);

            if (existingHash is not null) {
                return (null, new ConflictingHash(transactionHash, existingHash));
            }
        }

        var transaction = new Transaction { Id = Guid.NewGuid(), UniqueHash = transactionHash, Type = transactionType.Value, };
        mapper.Map(request, transaction);

        return (transaction, null);
    }

    public static (TransactionType?, InvalidRequest?) ValidateTransaction(TransactionRequest request) {
        if (!Enum.TryParse<TransactionType>(request.Type, out var transactionType)) {
            return (null, new InvalidRequest(
                "Type",
                $"Transaction type {request.Type} is invalid"
            ));
        }

        if (request.Amount < 0) {
            return (null, new InvalidRequest(
                "Amount",
                "Amount cannot be negative."
            ));
        }

        if (request.Category == "(none)") {
            return (null, new InvalidRequest(
                "Category",
                "Category cannot be '(none)'."
            ));
        }

        var problemDetail = (transactionType, request) switch {
            (TransactionType.Expense, { SourceAccountId: null })
                => "Expenses must have a source account",
            (TransactionType.Income, { DestinationAccountId: null })
                => "Income transactions must have a destination account",
            (TransactionType.Transfer, { SourceAccountId: null, } or { DestinationAccountId: null })
                => "Transfers must specify both and source account and destination account",
            (TransactionType.Transfer, { SourceAccountId: not null, DestinationAccountId: not null })
                when request.DestinationAccountId == request.SourceAccountId =>
                "Transfers cannot specify the same source and destination account",
            _ => null
        };

        return (
            transactionType,
            problemDetail is null
                ? null
                : new InvalidRequest(
                    "Transaction",
                    problemDetail
                )
        );
    }
}

public record ConflictingHash(string Hash, Transaction Transaction): Err();

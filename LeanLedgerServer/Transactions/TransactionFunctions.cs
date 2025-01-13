namespace LeanLedgerServer.Transactions;

using System.Diagnostics;
using AutoMapper;
using Automation;
using Common;
using Microsoft.EntityFrameworkCore;

public static class TransactionFunctions {
    // TODO: we will likely want to figure out how to express that a transaction has been affected by a rule
    public static async Task<Result<Transaction>> CreateNewTransaction(
        TransactionRequest request,
        IQueryable<Transaction> transactions,
        IMapper mapper,
        List<Rule>? rules = null
    ) => await ValidateTransaction(request)
        .ThenAsync(
            async transactionType => {
                var transactionHash = request.CreateHash();

                // ? In the future it might be a good idea to still check for a conflicting hash and add a (1) or something to it
                if (!request.SkipHashCheck) {
                    var existingHash = await transactions.FirstOrDefaultAsync(t => t.UniqueHash == transactionHash);

                    if (existingHash is not null) {
                        return new ConflictingHash(transactionHash, existingHash);
                    }
                }

                var transaction = new Transaction {
                    Id = Guid.NewGuid(),
                    UniqueHash = transactionHash,
                    Type = transactionType,
                    DateImported = request.ImportDate
                };
                mapper.Map(request, transaction);

                if (rules is not null) {
                    try {
                        foreach (var rule in rules.Where(r => r.ShouldTriggerFor(transaction))) {
                            rule.ApplyActionsTo(transaction);
                        }
                    } catch (Exception e) {
                        return new BadRule(e);
                    }
                }

                return transaction.AsResult();
            }
        );

    public static Result<TransactionType> ValidateTransaction(TransactionRequest request) {
        if (!Enum.TryParse<TransactionType>(request.Type, out var transactionType)) {
            return new InvalidRequest(
                "Type",
                $"Transaction type {request.Type} is invalid"
            );
        }

        if (request.Amount < 0) {
            return new InvalidRequest(
                "Amount",
                "Amount cannot be negative."
            );
        }

        if (request.Category == "(none)") {
            return new InvalidRequest(
                "Category",
                "Category cannot be '(none)'."
            );
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

        return problemDetail is null
            ? transactionType
            : new InvalidRequest("Transaction", problemDetail);
    }
}

public record ConflictingHash(string Hash, Transaction Transaction): Err() {
    public override IResult ToHttpResult() => Results.Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "Conflicting transaction has found",
        detail: $"Transaction already exists: {Hash}",
        extensions: new Dictionary<string, object?>() {
            {
                "hash", Hash
            }, {
                "existingTransaction", Transaction
            }
        }
    );
}

public record BadRule(Exception Exception): Err() {
    public override IResult ToHttpResult() => Results.Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Could not apply rules to transaction",
        detail: Exception.Message
    );
}

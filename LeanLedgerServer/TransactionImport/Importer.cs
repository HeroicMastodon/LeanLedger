namespace LeanLedgerServer.TransactionImport;

using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Accounts;
using AutoMapper;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Transactions;
using static Transactions.TransactionFunctions;

public class Importer(
    LedgerDbContext dbContext,
    IMapper mapper
) {
    // TODO: retest this post refactor
    // TODO: we will need some way to indicate that a transaction was affected by a rule
    // ? We could do a union thing
    // ? We could also keep a history of modifications to transactions
    public async Task<List<ImportedTransaction>> ImportCsv(
        Account account,
        ImportSettings settings,
        List<Dictionary<string, string?>> csv
    ) {
        var rules = await dbContext.Rules.ToListAsync();
        var transactions = new List<Result<Transaction>>();

        foreach (var (value, index) in csv.Enumerate()) {
            if (!MapInitialValues(settings, value, index).IsOk(out var mappedTransaction, out var error)) {
                transactions.Add(error);
                continue;
            }

            if (mappedTransaction.Amount is null) {
                transactions.Add(new ImportError(index, "Amount was not mapped for transaction"));
                continue;
            }

            if (mappedTransaction.Description is null) {
                transactions.Add(new ImportError(index, "Description was not mapped for transaction"));
                continue;
            }

            var transactionType = mappedTransaction.Amount >= 0
                ? TransactionType.Income
                : TransactionType.Expense;

            var request = new TransactionRequest(
                transactionType.ToString(),
                mappedTransaction.Description,
                mappedTransaction.Date ?? DateOnly.FromDateTime(DateTime.Now),
                Math.Abs(mappedTransaction.Amount.Value),
                transactionType == TransactionType.Expense ? account.Id : null,
                transactionType == TransactionType.Income ? account.Id : null,
                mappedTransaction.Category,
                ImportDate: DateOnly.FromDateTime(DateTime.Now)
            );

            var transaction = await CreateNewTransaction(
                    request,
                    dbContext.Transactions,
                    mapper,
                    rules
                )
                .ThenAsync(
                    async transaction => {
                        await dbContext.AddAsync(transaction);
                        return transaction;
                    }
                );

            transactions.Add(transaction);
        }

        await dbContext.SaveChangesAsync();

        return transactions
            .Select(
                (value, index) => value.Map(
                    ok: t => new ImportedTransaction(
                        ImportResult.Success,
                        index,
                        $"Successfully imported transaction {t.Description}",
                        t.Id
                    ),
                    error: e => e switch {
                        ConflictingHash err => new ImportedTransaction(
                            ImportResult.Failed,
                            index,
                            $"Transaction conflicts with existing transaction {err.Transaction}",
                            err.Transaction.Id
                        ),
                        ImportError err => new ImportedTransaction(
                            ImportResult.Failed,
                            index,
                            err.Message
                        ),
                        BadRule err => new ImportedTransaction(
                            ImportResult.Failed,
                            index,
                            err.Exception.Message
                        ),
                        _ => new ImportedTransaction(
                            ImportResult.Failed,
                            index,
                            $"Something went wrong while creating the transaction: {e}"
                        )
                    }
                )
            )
            .ToList();
    }

    private static Result<MappedTransaction> MapInitialValues(
        ImportSettings settings,
        Dictionary<string, string?> value,
        int index
    ) {
        var mappedTransaction = new MappedTransaction();

        foreach (var (columnName, field) in settings.ImportMappings) {
            var columnValue = value[columnName];
            if (string.IsNullOrWhiteSpace(columnValue)) {
                continue;
            }

            switch (field) {
                case ImportTransactionField.Amount: {
                    if (!decimal.TryParse(columnValue, CultureInfo.InvariantCulture, out var amount)) {
                        return new ImportError(index, $"Invalid number in amount field; {columnName}: {columnValue}");
                    }

                    mappedTransaction.Amount = amount;
                    break;
                }
                case ImportTransactionField.NegatedAmount: {
                    if (!decimal.TryParse(columnValue, CultureInfo.InvariantCulture, out var amount)) {
                        return new ImportError(index, $"Invalid number in amount field; {columnName}: {columnValue}");
                    }

                    mappedTransaction.Amount = amount * -1;
                    break;
                }
                case ImportTransactionField.Date:
                    if (!DateOnly.TryParseExact(columnValue, settings.DateFormat ?? "yyyy/mm/dd", out var date)) {
                        return new ImportError(index, $"Could not parse date; {columnName}: {columnValue}");
                    }

                    mappedTransaction.Date = date;
                    break;
                case ImportTransactionField.Description:
                    mappedTransaction.Description = columnValue;
                    break;
                case ImportTransactionField.Category:
                    mappedTransaction.Category = columnValue;
                    break;
                case ImportTransactionField.Ignore:
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        return mappedTransaction;
    }


    private class MappedTransaction {
        public string? Description { get; set; }
        public DateOnly? Date { get; set; }
        public decimal? Amount { get; set; }
        public string? Category { get; set; }
    }
}

public record ImportedTransaction(
    ImportResult Result,
    int Index,
    string Message,
    Guid? TransactionId = null
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImportResult {
    Success,
    Warning,
    Failed
}

public record ImportError(
    int? LineNumber,
    string Message
): Err() {
    public override IResult ToHttpResult() => Results.Problem(
        $"Could not import transaction at line {LineNumber?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}: {Message}",
        statusCode: StatusCodes.Status400BadRequest
    );
}

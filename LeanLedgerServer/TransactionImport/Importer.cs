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
    public async Task<List<ImportedTransaction>> ImportCsv(
        Account account,
        ImportSettings settings,
        List<Dictionary<string, string?>> csv
    ) {
        // This will execute in parallel and may cause some data race conditions
        // Likely we need to refactor to a more simple for each loop
        var rules = await dbContext.Rules.ToListAsync();
        var transactions = new List<Result<Transaction>>();
        foreach (var (value, index) in csv.Enumerate()) {
            var mappedTransaction = new MappedTransaction();
            foreach (var (columnName, field) in settings.ImportMappings) {
                var columnValue = value[columnName];
                if (string.IsNullOrWhiteSpace(columnValue)) {
                    continue;
                }

                switch (field) {
                    case TransactionImportField.Amount: {
                        if (!decimal.TryParse(columnValue, CultureInfo.InvariantCulture, out var amount)) {
                            transactions.Add(new ImportError(index, $"Invalid number in amount field; {columnName}: {columnValue}"));
                        }

                        mappedTransaction.Amount = amount;
                        break;
                    }
                    case TransactionImportField.NegatedAmount: {
                        if (!decimal.TryParse(columnValue, CultureInfo.InvariantCulture, out var amount)) {
                            transactions.Add(new ImportError(index, $"Invalid number in amount field; {columnName}: {columnValue}"));
                        }

                        mappedTransaction.Amount = amount * -1;
                        break;
                    }
                    case TransactionImportField.Date:
                        if (!DateOnly.TryParseExact(columnValue, settings.DateFormat ?? "yyyy/mm/dd", out var date)) {
                            transactions.Add(new ImportError(index, $"Could not parse date; {columnName}: {columnValue}"));
                        }

                        mappedTransaction.Date = date;
                        break;
                    case TransactionImportField.Description:
                        mappedTransaction.Description = columnValue;
                        break;
                    case TransactionImportField.Category:
                        mappedTransaction.Category = columnValue;
                        break;
                    case TransactionImportField.Ignore:
                        break;
                    default:
                        throw new UnreachableException();
                }
            }

            if (mappedTransaction.Amount is null) {
                transactions.Add(new ImportError(index, "Amount was not mapped for transaction"));
            }

            if (mappedTransaction.Description is null) {
                transactions.Add(new ImportError(index, "Description was not mapped for transaction"));
            }

            var transactionType = TransactionType.Income;
            if (mappedTransaction.Amount < 0) {
                transactionType = TransactionType.Expense;
                mappedTransaction.Amount *= -1;
            }

            var request = new TransactionRequest(
                transactionType.ToString(),
                mappedTransaction.Description ?? "",
                mappedTransaction.Date ?? DateOnly.FromDateTime(DateTime.Now),
                mappedTransaction.Amount!.Value,
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

        return transactions.Enumerate()
            .Select(
                pair => pair.Value.Map(
                    ok: t => new ImportedTransaction(
                        ImportResult.Success,
                        pair.Index,
                        $"Successfully imported transaction {t.Description}",
                        t.Id
                    ),
                    error: e => e switch {
                        ConflictingHash err => new ImportedTransaction(
                            ImportResult.Failed,
                            pair.Index,
                            $"Transaction conflicts with existing transaction {err.Transaction}",
                            err.Transaction.Id
                        ),
                        ImportError err => new ImportedTransaction(
                            ImportResult.Failed,
                            pair.Index,
                            err.Message
                        ),
                        BadRule err => new ImportedTransaction(
                            ImportResult.Failed,
                            pair.Index,
                            err.Exception.Message
                        ),
                        _ => new ImportedTransaction(
                            ImportResult.Failed,
                            pair.Index,
                            $"Something went wrong while creating the transaction: {e}"
                        )
                    }
                )
            )
            .ToList();
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

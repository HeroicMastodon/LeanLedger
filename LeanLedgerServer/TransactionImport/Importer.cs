namespace LeanLedgerServer.TransactionImport;

using System.Diagnostics;
using System.Globalization;
using Accounts;
using AutoMapper;
using Common;
using Transactions;
using static Transactions.TransactionFunctions;

public class Importer(
    LedgerDbContext dbContext,
    IMapper mapper
) {
    public async Task<Result<ImportOutput>> ImportCsv(
        Account account,
        ImportSettings settings,
        List<Dictionary<string, string?>> csv
    ) {
        // This will execute in parallel and may cause some data race conditions
        // Likely we need to refactor to a more simple for each loop
        var transactions = new List<Result<Transaction>>();
        foreach (var (value, index) in csv.Enumerate()) {
            var mappedTransaction = new MappedTransaction();
            foreach (var (columnName, field) in settings.ImportMappings) {
                var columnValue = value[columnName];
                if (string.IsNullOrWhiteSpace(columnValue)) {
                    continue;
                }

                switch (field) {
                    case TransactionField.Amount: {
                        if (!decimal.TryParse(columnValue, CultureInfo.InvariantCulture, out var amount)) {
                            transactions.Add(new ImportError(index, $"Invalid number in amount field; {columnName}: {columnValue}"));
                        }

                        mappedTransaction.Amount = amount;
                        break;
                    }
                    case TransactionField.NegatedAmount: {
                        if (!decimal.TryParse(columnValue, CultureInfo.InvariantCulture, out var amount)) {
                            transactions.Add(new ImportError(index, $"Invalid number in amount field; {columnName}: {columnValue}"));
                        }

                        mappedTransaction.Amount = amount * -1;
                        break;
                    }
                    case TransactionField.Date:
                        if (!DateOnly.TryParseExact(columnValue, settings.DateFormat ?? "yyyy/mm/dd", out var date)) {
                            transactions.Add(new ImportError(index, $"Could not parse date; {columnName}: {columnValue}"));
                        }

                        mappedTransaction.Date = date;
                        break;
                    case TransactionField.Description:
                        mappedTransaction.Description = columnValue;
                        break;
                    case TransactionField.Category:
                        mappedTransaction.Category = columnValue;
                        break;
                    case TransactionField.Ignore:
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
                    mapper
                )
                .MapError(err => new ImportError(index, $"Problem occurred while creating transaction: {err}"))
                .ThenAsync(
                    async transaction => {
                        await dbContext.AddAsync(transaction);
                        return transaction;
                    }
                );

            transactions.Add(transaction);
        }

        await dbContext.SaveChangesAsync();

        return transactions.Aggregate(
            new ImportOutput(
                Imported: [],
                Errors: []
            ),
            (value, result) => {
                result.When(
                    ok: value.Imported.Add,
                    error: err
                        => value.Errors.Add(err as ImportError ?? new ImportError(null, err.ToString()))
                );
                return value;
            }
        );
    }


    private class MappedTransaction {
        public string? Description { get; set; }
        public DateOnly? Date { get; set; }
        public decimal? Amount { get; set; }
        public string? Category { get; set; }
    }
}

public record ImportOutput(
    List<Transaction> Imported,
    List<ImportError> Errors
);

public record ImportError(
    int? LineNumber,
    string Message
): Err() {
    public override IResult ToHttpResult() => Results.Problem(
        $"Could not import transaction at line {LineNumber?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}: {Message}",
        statusCode: StatusCodes.Status400BadRequest
    );
}

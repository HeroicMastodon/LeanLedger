namespace LeanLedgerServer.TransactionImport;

using System.Diagnostics;
using System.Globalization;
using Accounts;
using AutoMapper;
using Common;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;
using static ImportFunctions;
using static Results;
using static Transactions.TransactionFunctions;

public static class Endpoints {
    public static void MapImport(this RouteGroupBuilder routes) {
        var imports = routes.MapGroup("imports");
        imports
            .MapPost("{accountId:guid}", RunImport)
            .DisableAntiforgery();

        var settings = imports.MapGroup("settings");
        settings.MapGet("{accountId:guid}", GetImportSettings);
        settings.MapPut("{accountId:guid}", UpdateImportSettings);
    }

    private static async Task<IResult> RunImport(
        Guid accountId,
        IFormFile csvFile,
        [FromServices]
        LedgerDbContext dbContext,
        [FromServices]
        IMapper mapper
    ) {
        var (account, err) = await GetValidAccount(accountId, dbContext);
        if (err is not null) {
            return err;
        }

        var settings = await EnsureSettingsExist(account!, dbContext);
        var csv = await ReadCsvToList(csvFile);

        List<MappedTransaction> mappedTransactions = [];
        foreach (var line in csv) {
            var mappedTransaction = new MappedTransaction();
            foreach (var (columnName, field) in settings.ImportMappings) {
                var columnValue = line[columnName];
                if (columnValue is null) {
                    continue;
                }

                switch (field) {
                    case TransactionField.Amount:
                        mappedTransaction.Amount = decimal.Parse(
                            columnValue,
                            CultureInfo.CurrentCulture
                        );
                        break;
                    case TransactionField.NegatedAmount:
                        mappedTransaction.Amount = decimal.Parse(columnValue, CultureInfo.CurrentCulture) * -1;
                        break;
                    case TransactionField.Date:
                        mappedTransaction.Date = DateOnly.ParseExact(columnValue, settings.DateFormat ?? "yyyy/mm/dd");
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

            // TODO: I need more precise error handling so I can still move on
            if (mappedTransaction.Amount is null) {
                throw new Exception("how could this have happened?");
            }

            mappedTransactions.Add(mappedTransaction);
        }

        var transactionTasks = mappedTransactions.Select(
                tr => {
                    var transactionType = TransactionType.Income;
                    if (tr.Amount < 0) {
                        transactionType = TransactionType.Expense;
                        tr.Amount *= -1;
                    }

                    return new TransactionRequest(
                        transactionType.ToString(),
                        tr.Description ?? "",
                        tr.Date ?? DateOnly.FromDateTime(DateTime.Now),
                        tr.Amount!.Value,
                        transactionType == TransactionType.Expense ? accountId : null,
                        transactionType == TransactionType.Income ? accountId : null,
                        tr.Category
                    );
                }
            )
            .Select(
                tr => CreateNewTransaction(
                    tr,
                    dbContext.Transactions,
                    mapper
                )
            );
        var transactions = await Task.WhenAll(transactionTasks);
        foreach (var (transaction, _) in transactions)
        {
            if (transaction is null) {
                continue;
            }

            await dbContext.AddAsync(transaction);
        }
        await dbContext.SaveChangesAsync();

        return Ok(transactions.Select(res => res.Item1));
    }

    private static async Task<IResult> GetImportSettings(
        Guid accountId,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var (account, err) = await GetValidAccount(accountId, dbContext);

        if (err is not null) {
            return err;
        }

        var settings = await EnsureSettingsExist(account!, dbContext);

        return Ok(new { settings.DateFormat, settings.CsvDelimiter, settings.ImportMappings });
    }

    private static async Task<IResult> UpdateImportSettings(
        Guid accountId,
        [FromServices]
        LedgerDbContext dbContext,
        [FromBody]
        ImportSettingsRequest request
    ) {
        var (account, err) = await GetValidAccount(accountId, dbContext);

        if (err is not null) {
            return err;
        }

        var settings = await EnsureSettingsExist(account!, dbContext);
        settings.CsvDelimiter = request.CsvDelimiter;
        settings.DateFormat = request.DateFormat;
        settings.ImportMappings = request.ImportMappings;

        dbContext.Update(settings);
        await dbContext.SaveChangesAsync();

        return Ok(new { settings.DateFormat, settings.CsvDelimiter, settings.ImportMappings });
    }

    private static async Task<(Account?, IResult?)> GetValidAccount(Guid accountId, LedgerDbContext dbContext) {
        // merchants cannot have imports
        var account = await dbContext.Accounts
            .Where(a => a.Id == accountId)
            .FirstOrDefaultAsync();

        if (account is null) {
            return (null, Problem($"Could not find an account with id {accountId}", statusCode: StatusCodes.Status404NotFound));
        }

        var isMerchant = account.AccountType == AccountType.Merchant;

        if (isMerchant) {
            return (null, Problem(
                "Merchants cannot have an import configuration",
                statusCode: StatusCodes.Status400BadRequest
            ));
        }

        return (account, null);
    }

    private static async Task<ImportSettings> EnsureSettingsExist(Account account, LedgerDbContext dbContext) {
        // create the import settings if they don't exist yet
        var settings = await dbContext.ImportSettings
            .Include(s => s.AttachedAccount)
            .FirstOrDefaultAsync(s => s.AttachedAccountId == account.Id);

        if (settings is null) {
            settings = CreateDefaultSettings(account!);
            await dbContext.AddAsync(settings);
            await dbContext.SaveChangesAsync();
        }

        return settings;
    }

    private static ImportSettings CreateDefaultSettings(Account account) => new() { AttachedAccount = account, AttachedAccountId = account.Id, ImportMappings = [], CsvDelimiter = ',' };

    private record ImportSettingsRequest(
        char? CsvDelimiter,
        string? DateFormat,
        List<ImportMapping> ImportMappings
    );

    private class MappedTransaction {
        public string? Description { get; set; }
        public DateOnly? Date { get; set; }
        public decimal? Amount { get; set; }
        public string? Category { get; set; }
    }
}

namespace LeanLedgerServer.TransactionImport;

using Accounts;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ImportFunctions;
using static Results;

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
        IMapper mapper,
        [FromServices]
        Importer importer
    ) {

        return await GetValidAccount(accountId, dbContext)
            .ThenAsync(
                async account => {
                    var settings = await EnsureSettingsExist(account, dbContext);
                    var csv = await ReadCsvToList(csvFile);

                    return await importer.ImportCsv(account, settings, csv);
                }
            )
            .Map(
                ok: Ok,
                error: err => err.ToHttpResult()
            );
    }

    private static async Task<IResult> GetImportSettings(
        Guid accountId,
        [FromServices]
        LedgerDbContext dbContext
    ) => await GetValidAccount(accountId, dbContext)
        .MapAsync(
            ok: async account => {
                var settings = await EnsureSettingsExist(account!, dbContext);

                return Ok(
                    new {
                        settings.DateFormat,
                        settings.CsvDelimiter,
                        settings.ImportMappings
                    }
                );
            },
            error: error => Task.FromResult(error.ToHttpResult())
        );

    private static async Task<IResult> UpdateImportSettings(
        Guid accountId,
        [FromServices]
        LedgerDbContext dbContext,
        [FromBody]
        ImportSettingsRequest request
    ) => await GetValidAccount(accountId, dbContext)
        .MapAsync(
            ok: async account => {
                var settings = await EnsureSettingsExist(account!, dbContext);
                settings.CsvDelimiter = request.CsvDelimiter;
                settings.DateFormat = request.DateFormat;
                settings.ImportMappings = request.ImportMappings;

                dbContext.Update(settings);
                await dbContext.SaveChangesAsync();

                return Ok(
                    new {
                        settings.DateFormat,
                        settings.CsvDelimiter,
                        settings.ImportMappings
                    }
                );
            },
            error: error => Task.FromResult(error.ToHttpResult())
        );

    private static async Task<Result<Account>> GetValidAccount(Guid accountId, LedgerDbContext dbContext) {
        // merchants cannot have imports
        var account = await dbContext.Accounts
            .Where(a => a.Id == accountId)
            .FirstOrDefaultAsync();

        if (account is null) {
            return new AccountNotFound(accountId);
        }

        var isMerchant = account.AccountType == AccountType.Merchant;

        if (isMerchant) {
            return new InvalidImportAccountType(account.AccountType);
        }

        return account;
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

    private static ImportSettings CreateDefaultSettings(Account account) => new() {
        AttachedAccount = account,
        AttachedAccountId = account.Id,
        ImportMappings = [],
        CsvDelimiter = ','
    };

    private record ImportSettingsRequest(
        char? CsvDelimiter,
        string? DateFormat,
        List<ImportMapping> ImportMappings
    );

    private record InvalidImportAccountType(AccountType Type): Err() {
        public override IResult ToHttpResult() => Problem(
            $"{Type}s cannot have an import configuration",
            statusCode: StatusCodes.Status400BadRequest
        );
    }
}

namespace LeanLedgerServer.TransactionImport;

using Accounts;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Results;

public static class Endpoints {
    public static void MapImport(this RouteGroupBuilder routes) {
        var imports = routes.MapGroup("imports");
        var settings = imports.MapGroup("settings");
        settings.MapGet("{accountId:guid}", GetImportSettings);
        settings.MapPut("{accountId:guid}", UpdateImportSettings);
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

    private static ImportSettings CreateDefaultSettings(Account account) => new() { AttachedAccount = account, AttachedAccountId = account.Id, ImportMappings = [], CsvDelimiter = ','};

    private record ImportSettingsRequest(
        char? CsvDelimiter,
        string? DateFormat,
        List<ImportMapping> ImportMappings
    );
}

namespace LeanLedgerServer.Accounts;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Results;

public static class Endpoints {
    public static void MapAccounts(this RouteGroupBuilder app) {
        var accounts = app.MapGroup("accounts");
        accounts.MapGet("", ListAccounts);
        accounts.MapGet("{id:guid}", GetAccount);
        accounts.MapPost("", CreateAccount);
        accounts.MapPut("{id:guid}", UpdateAccount);
        accounts.MapDelete("{id:guid}", DeleteAccount);
    }

    private static async Task<IResult> ListAccounts([FromServices] LedgerDbContext dbContext) {
        var accounts = await dbContext.Accounts.ToListAsync();
        return Ok(new {Accounts = accounts});
    }

    private static async Task<IResult> GetAccount(Guid id, [FromServices] LedgerDbContext dbContext) {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == id);

        return Ok(account);
    }

    private static async Task<IResult> CreateAccount(
        [FromBody]
        AccountRequest newAccount,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        if (!Enum.TryParse<AccountType>(newAccount.AccountType, out var accountType)) {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid account type",
                detail: $"Account type '{newAccount.AccountType}' is not supported"
            );
        }

        var account = new Account {
            Id = Guid.NewGuid(),
            Name = newAccount.Name,
            AccountType = accountType,
            OpeningBalance = newAccount.OpeningBalance,
            // OpeningDate = DateOnly.FromDateTime(newAccount.OpeningDate),
            OpeningDate = newAccount.OpeningDate,
            Active = newAccount.Active,
            Notes = newAccount.Notes
        };

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        return Ok(account);
    }

    private static async Task<IResult> UpdateAccount(
        Guid id,
        [FromBody]
        AccountRequest accountUpdate,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == id);

        if (account is null) {
            return NotFound();
        }

        if (!Enum.TryParse<AccountType>(accountUpdate.AccountType, out var accountType)) {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid account type",
                detail: $"Account type '{accountUpdate.AccountType}' is not supported"
            );
        }

        account.Name = accountUpdate.Name;
        account.AccountType = accountType;
        account.OpeningBalance = accountUpdate.OpeningBalance;
        // account.OpeningDate = DateOnly.FromDateTime(accountUpdate.OpeningDate);
        account.OpeningDate = accountUpdate.OpeningDate;
        account.Active = accountUpdate.Active;
        account.Notes = accountUpdate.Notes;

        dbContext.Update(account);
        await dbContext.SaveChangesAsync();

        return Ok(account);
    }

    private static async Task<IResult> DeleteAccount(Guid id, [FromServices] LedgerDbContext dbContext) {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Id == id);

        if (account is null) {
            return NoContent();
        }

        account.IsDeleted = false;
        dbContext.Update(account);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private record AccountRequest(
        string Name,
        string AccountType,
        decimal OpeningBalance,
        DateOnly OpeningDate,
        bool Active,
        string Notes
    );
}

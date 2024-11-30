namespace LeanLedgerServer.Accounts;

using System.Collections.Immutable;
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
        var accounts = await dbContext.Accounts
            .Select(
                a => new {
                    a.Id,
                    a.AccountType,
                    a.Name,
                    a.Active,
                    a.IncludeInNetWorth,
                    Balance = a.Withdrawls.Sum(t => t.Amount) + a.Deposits.Sum(t => t.Amount) + a.OpeningBalance,
                    // TODO: This will probably be better as a separate query in the future
                    LastActivityDate = a.Withdrawls.Any()
                        ? (DateOnly?)a.Withdrawls.OrderByDescending(t => t.Date).First().Date
                        : null,
                    BalanceChange = a.Withdrawls.Sum(t => t.Amount) + a.Deposits.Sum(t => t.Amount),
                }
            )
            .ToListAsync();
        var grouped = accounts
            .GroupBy(a => a.AccountType)
            .ToDictionary(group => group.Key, group => group);
        return Ok(grouped);
    }

    private static async Task<IResult> GetAccount(Guid id, [FromServices] LedgerDbContext dbContext) {
        var account = await dbContext
            .Accounts
            .Include(a => a.Withdrawls)
            .ThenInclude(t => t.DestinationAccount)
            .Include(a => a.Deposits)
            .ThenInclude(t => t.SourceAccount)
            .SingleOrDefaultAsync(a => a.Id == id);

        if (account is null) {
            return NotFound();
        }

        return Ok(
            new {
                account.Id,
                account.Name,
                AccountType = account.AccountType.ToString(),
                account.OpeningBalance,
                account.OpeningDate,
                account.Active,
                account.IncludeInNetWorth,
                account.Notes,
                Balance = account.Withdrawls.Sum(t => t.Amount) +
                          account.Deposits.Sum(t => t.Amount) +
                          account.OpeningBalance,
                Transactions = account
                    .Withdrawls
                    .ToImmutableArray()
                    .AddRange(account.Deposits)
                    .Select(
                        t => new {
                            t.Id,
                            t.Description,
                            t.Amount,
                            t.Date,
                            t.Category,
                            SourceAccount = new { t.SourceAccount?.Id, t.SourceAccount?.Name },
                            DestinationAccount = new { t.DestinationAccount?.Id, t.DestinationAccount?.Name },
                        }
                    )
            }
        );
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
            IncludeInNetWorth = newAccount.IncludeInNetWorth,
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
        account.IncludeInNetWorth = accountUpdate.IncludeInNetWorth;
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

        account.IsDeleted = true;
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
        bool IncludeInNetWorth,
        string Notes
    );
}

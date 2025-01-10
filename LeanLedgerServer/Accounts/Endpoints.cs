namespace LeanLedgerServer.Accounts;

using System.Collections.Immutable;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;
using static Results;

public static class Endpoints {
    public static void MapAccounts(this RouteGroupBuilder app) {
        var accounts = app.MapGroup("accounts");
        accounts.MapGet("", ListAccounts);
        accounts.MapGet("{id:guid}", GetAccount);
        accounts.MapPost("", CreateAccount);
        accounts.MapPut("{id:guid}", UpdateAccount);
        accounts.MapDelete("{id:guid}", DeleteAccount);
        accounts.MapGet("options", ListAccountOptions);
    }

    private static async Task<IResult> ListAccounts([FromServices] LedgerDbContext dbContext) {
        var accounts = await dbContext.Accounts
            .Include(a => a.Withdrawls)
            .Include(a => a.Deposits)
            .Select(
                a => new {
                    a.Id,
                    a.AccountType,
                    a.Name,
                    a.Active,
                    a.IncludeInNetWorth,
                    Balance = CalculateBalance(a),
                    LastActivityDate = a.Withdrawls.Count != 0
                        ? a.Withdrawls.OrderByDescending(t => t.Date).First().Date
                        : a.Deposits.Count != 0
                            ? (DateOnly?)a.Deposits.OrderByDescending(t => t.Date).First().Date
                            : null,
                    BalanceChange = a.Deposits.Sum(t => t.Amount) - a.Withdrawls.Sum(t => t.Amount),
                }
            )
            .ToListAsync();
        var grouped = accounts
            .GroupBy(a => a.AccountType)
            .ToDictionary(group => group.Key, group => group);
        return Ok(grouped);
    }

    private static decimal CalculateBalance(Account a) =>
        a.Deposits.Sum(t => t.Amount) -
        a.Withdrawls.Sum(t => t.Amount) +
        a.OpeningBalance;


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
                Balance = CalculateBalance(account),
                Transactions = account
                    .Withdrawls
                    .ToImmutableArray()
                    .AddRange(account.Deposits)
                    .Select(TableTransaction.FromTransaction)
            }
        );
    }

    private static async Task<IResult> CreateAccount(
        [FromBody]
        AccountRequest newAccount,
        [FromServices]
        LedgerDbContext dbContext,
        [FromServices]
        IMapper mapper
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
            AccountType = accountType,
        };
        mapper.Map(newAccount, account);

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        return Ok(account);
    }

    private static async Task<IResult> UpdateAccount(
        Guid id,
        [FromBody]
        AccountRequest accountUpdate,
        [FromServices]
        LedgerDbContext dbContext,
        [FromServices]
        IMapper mapper
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

        account.AccountType = accountType;
        mapper.Map(accountUpdate, account);

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

    // TODO: Move this to the completions controller and update the front end
    private static async Task<IResult> ListAccountOptions([FromServices] LedgerDbContext dbContext) {
        var accounts = await dbContext.Accounts
            .OrderBy(a => a.AccountType)
            .Select(a => new { Name = string.Concat(a.Name, " - (", a.AccountType.ToString(), ")"), a.Id })
            .ToListAsync();

        return Ok(accounts);
    }
}

public record AccountRequest(
    string Name,
    string AccountType,
    decimal OpeningBalance,
    DateOnly OpeningDate,
    bool Active,
    bool IncludeInNetWorth,
    string Notes
);

public class AccountProfile: Profile {
    public AccountProfile() {
        CreateMap<AccountRequest, Account>()
            .ForMember(a => a.AccountType, opt => opt.Ignore());
    }
}

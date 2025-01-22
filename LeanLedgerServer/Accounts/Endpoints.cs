namespace LeanLedgerServer.Accounts;

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

    private static async Task<IResult> ListAccounts(
        [AsParameters]
        QueryByMonth queryByMonth,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var accounts = await dbContext.Accounts.ToArrayAsync();
        var mappedAccounts = new List<dynamic>();
        foreach (var account in accounts) {
            var balanceChange = await QueryBalanceChange(queryByMonth, dbContext, account);

            mappedAccounts.Add(
                new {
                    account.Id,
                    account.AccountType,
                    account.Name,
                    account.Active,
                    account.IncludeInNetWorth,
                    Balance = balanceChange + account.OpeningBalance,
                    BalanceChange = balanceChange,
                    LastActivityDate = await dbContext.Accounts
                        .Include(a => a.Withdrawls)
                        .Include(a => a.Deposits)
                        .Where(a => a.Id == account.Id)
                        .Select(
                            a => a.Withdrawls.Count != 0
                                ? a.Withdrawls.OrderByDescending(t => t.Date).First().Date
                                : a.Deposits.Count != 0
                                    ? (DateOnly?)a.Deposits.OrderByDescending(t => t.Date).First().Date
                                    : null
                        )
                        .SingleAsync(),
                }
            );
        }

        var grouped = mappedAccounts
            .GroupBy(a => a.AccountType)
            .ToDictionary(group => group.Key, group => group.OrderBy(a => a.Name).ToArray());
        return Ok(grouped);
    }


    private static async Task<IResult> GetAccount(
        Guid id,
        [AsParameters]
        QueryByMonth byMonth,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var account = await dbContext
            .Accounts
            .SingleOrDefaultAsync(a => a.Id == id);

        if (account is null) {
            return NotFound();
        }

        var balanceChange = await QueryBalanceChange(byMonth, dbContext, account);

        return Ok(
            new {
                account.Id,
                account.Name,
                account.AccountType,
                account.OpeningBalance,
                account.OpeningDate,
                account.Active,
                account.IncludeInNetWorth,
                account.Notes,
                Balance = balanceChange + account.OpeningBalance,
                Transactions = (
                        await QueryTransactionsByMonth(
                                dbContext.Transactions,
                                account,
                                byMonth,
                                givenMonthOnly: true
                            )
                            .OrderByDescending(t => t.Date)
                            .ToArrayAsync()
                    )
                    .Select(TableTransaction.FromTransaction)
            }
        );
    }

    private static async Task<decimal> QueryBalanceChange(
        QueryByMonth queryByMonth,
        LedgerDbContext dbContext,
        Account account
    ) =>
        await QueryTransactionsByMonth(dbContext.Transactions, account, queryByMonth)
            .Where(t => t.Date >= account.OpeningDate)
            .Select(t => t.SourceAccountId == account.Id ? t.Amount * -1 : t.Amount)
            .SumAsync();

    private static IQueryable<Transaction> QueryTransactionsByMonth(
        IQueryable<Transaction> transactions,
        Account account,
        QueryByMonth byMonth,
        bool givenMonthOnly = false
    ) {
        var query = transactions
            .Where(t => t.SourceAccountId == account.Id || t.DestinationAccountId == account.Id);

        return byMonth is { Month: not null, Year: not null }
            ? givenMonthOnly
                ? query.Where(
                    t => t.Date.Year == byMonth.Year && t.Date.Month == byMonth.Month
                )
                : query.Where(
                    t => (t.Date.Year < byMonth.Year
                          || (t.Date.Year == byMonth.Year && t.Date.Month <= byMonth.Month))
                )
            : query;
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

    private static async Task<IResult> ListAccountOptions([FromServices] LedgerDbContext dbContext) {
        var accounts = await dbContext.Accounts
            .OrderBy(a => a.AccountType)
            .Select(
                a => new {
                    Name = string.Concat(a.Name, " - (", a.AccountType.ToString(), ")"),
                    a.Id
                }
            )
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

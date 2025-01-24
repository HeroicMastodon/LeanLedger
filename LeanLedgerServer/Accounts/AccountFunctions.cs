namespace LeanLedgerServer.Accounts;

using Common;
using Microsoft.EntityFrameworkCore;
using static Common.QueryByMonthFunctions;

public static class AccountFunctions {
    public static async Task<decimal> QueryBalanceChangeByMonth(
        QueryByMonth queryByMonth,
        LedgerDbContext dbContext,
        Account account
    ) =>
        (await QueryTransactionsByMonth(dbContext.Transactions, queryByMonth)
            .Where(t => t.SourceAccountId == account.Id || t.DestinationAccountId == account.Id)
            .Where(t => t.Date >= account.OpeningDate)
            .Select(t => t.SourceAccountId == account.Id ? t.Amount * -1 : t.Amount)
            .SumAsync())
        + account.OpeningBalance;
}

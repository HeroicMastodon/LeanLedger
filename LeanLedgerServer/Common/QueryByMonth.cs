namespace LeanLedgerServer.Common;

using Microsoft.AspNetCore.Mvc;
using Transactions;

public record QueryByMonth(
    [FromQuery]
    int? Month,
    [FromQuery]
    int? Year
) {
    public bool IsValidQuery() => this is { Year: not null, Month: not null };
}

public static class QueryByMonthFunctions {
    public static IQueryable<Transaction> QueryTransactionsByMonth(
        IQueryable<Transaction> query,
        QueryByMonth byMonth,
        bool givenMonthOnly = false
    ) =>
        byMonth is { Month: not null, Year: not null }
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

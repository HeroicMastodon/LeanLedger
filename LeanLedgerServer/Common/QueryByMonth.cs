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

    public QueryByMonth Increment() => Month == 12
        ? new QueryByMonth(1, Year + 1)
        : this with {
            Month = Month + 1
        };
    public QueryByMonth Decrement() => Month == 1
        ? new QueryByMonth(12, Year - 1)
        : this with {
            Month = Month - 1
        };
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

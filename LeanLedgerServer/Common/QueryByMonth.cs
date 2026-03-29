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

    public QueryByMonth IncrementByYear(int years = 1) => this with {
        Year = Year + years
    };

    public QueryByMonth Decrement(int months = 1) {
        if (months < 0) {
            throw new ArgumentException("Months to decrement must be non-negative", nameof(months));
        }

        var yearsToDecrement = months / 12;
        var remainingMonths = months % 12;

        if (Month <= remainingMonths) {
            return this with {
                Month = 12 - (remainingMonths - Month),
                Year = Year - yearsToDecrement - 1
            };
        } else {
            return this with {
                Month = Month - remainingMonths,
                Year = Year - yearsToDecrement
            };
        }
    }

    public QueryByMonth DecrementByYear(int years = 1) => this with {
        Year = Year - years
    };

    public QueryByMonth DecrementByQuarter(int quarters = 1) => Decrement(quarters * 3);

    public string MonthName => Month switch {
        1 => "Jan",
        2 => "Feb",
        3 => "Mar",
        4 => "Apr",
        5 => "May",
        6 => "Jun",
        7 => "Jul",
        8 => "Aug",
        9 => "Sep",
        10 => "Oct",
        11 => "Nov",
        12 => "Dec",
        _ => throw new InvalidOperationException($"Invalid month: {Month}")
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
                    t => t.Date.Year < byMonth.Year
                          || (t.Date.Year == byMonth.Year && t.Date.Month <= byMonth.Month)
                )
            : query;
}

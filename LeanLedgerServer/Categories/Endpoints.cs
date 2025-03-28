namespace LeanLedgerServer.Categories;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;
using static Common.QueryByMonthFunctions;
using static Results;

public static class Endpoints {
    private const string NULL_CATEGORY_NAME = "(none)";

    public static void MapCategories(this IEndpointRouteBuilder endpoints) {
        var categories = endpoints.MapGroup("categories");
        categories.MapGet("options", ListCategoryOptions);
        categories.MapGet("", ListCategories);
        categories.MapGet("{category}", GetCategory);
    }

    private static async Task<IResult> GetCategory(
        string category,
        [AsParameters]
        QueryByMonth byMonth,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var query = dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.Type == TransactionType.Expense);

        query = QueryTransactionsByMonth(query, byMonth, givenMonthOnly: true);
        query = category == NULL_CATEGORY_NAME
            ? query.Where(t => t.Category == null || t.Category == "")
            : query.Where(t => t.Category == category);

        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return Ok(
            new {
                Name = category,
                Transactions = transactions.Select(
                    TableTransaction.FromTransaction
                ),
                Amount = transactions.Sum(
                    TransactionAmount
                )
            }
        );
    }


    private static async Task<IResult> ListCategories(
        [AsParameters]
        QueryByMonth byMonth,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        // ? Watch this for performance issues
        var transactions = await QueryTransactionsByMonth(dbContext.Transactions, byMonth, givenMonthOnly: true)
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .ToListAsync();

        return Ok(
            transactions.Select(
                    g => new {
                        Name = string.IsNullOrWhiteSpace(g.Key)
                            ? NULL_CATEGORY_NAME
                            : g.Key,
                        Amount = g.Select(TransactionAmount).Sum()
                    }
                )
                .OrderBy(t => t.Name)
        );
    }

    private static decimal TransactionAmount(Transaction t) =>
        t.Type is TransactionType.Income
            ? t.Amount
            : -1 * t.Amount;

    private static async Task<IResult> ListCategoryOptions(
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var results = await dbContext.Transactions
            .Select(t => t.Category)
            .Distinct()
            .ToListAsync();

        return Ok(results);
    }
}

namespace LeanLedgerServer.Categories;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions;
using static Results;

public static class Endpoints {
    public static void MapCategories(this IEndpointRouteBuilder endpoints) {
        var categories = endpoints.MapGroup("categories");
        categories.MapGet("options", ListCategoryOptions);
        categories.MapGet("", ListCategories);
        categories.MapGet("{category}", GetCategory);
    }

    private static async Task<IResult> GetCategory(
        string category,
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var transactions = await dbContext.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Where(t => t.Category == category)
            .ToListAsync();

        return Ok(new { Category = category, Transactions = transactions, });
    }

    private static async Task<IResult> ListCategories(
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var transactions = await dbContext.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Amount = g.Select(t => t.Amount).Sum() })
            .ToListAsync();

        return Ok(transactions);
    }

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

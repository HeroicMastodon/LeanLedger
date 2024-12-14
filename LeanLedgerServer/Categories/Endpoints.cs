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
        var categoryToCompare = category == "(none)" ? "" : category;
        var transactions = await dbContext.Transactions
            .Include(t => t.SourceAccount)
            .Include(t => t.DestinationAccount)
            .Where(t => t.Category == categoryToCompare)
            .ToListAsync();

        return Ok(
            new {
                Name = category,
                Transactions = transactions.Select(
                    t => new {
                        t.Id,
                        t.Description,
                        t.Amount,
                        t.Date,
                        t.Category,
                        Type = t.Type.ToString(),
                        SourceAccount = new { t.SourceAccount?.Id, t.SourceAccount?.Name },
                        DestinationAccount = new { t.DestinationAccount?.Id, t.DestinationAccount?.Name },
                    }
                ),
                Amount = transactions.Sum(t => t.Type is TransactionType.Income ? t.Amount : -1 * t.Amount)
            }
        );
    }

    private static async Task<IResult> ListCategories(
        [FromServices]
        LedgerDbContext dbContext
    ) {
        var transactions = await dbContext.Transactions
            .GroupBy(t => t.Category)
            .Select(g => new { Name = string.IsNullOrWhiteSpace(g.Key) ? "(none)" : g.Key, Amount = g.Select(t => t.Type == TransactionType.Income ? t.Amount : -1 * t.Amount).Sum() })
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

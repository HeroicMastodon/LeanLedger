namespace LeanLedgerServer.Categories;

using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Results;

public static class Endpoints {
    public static void MapCategories(this IEndpointRouteBuilder endpoints) {
        var transactions = endpoints.MapGroup("categories");
        transactions.MapGet("options", ListCategoryOptions);
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

using LeanLedgerServer.Accounts;
using LeanLedgerServer.Common;
using LeanLedgerServer.Transactions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = "StaticFiles" });
builder.Services.AddDbContext<LedgerDbContext>(
    options => options.UseSqlite("Data Source=ledger.db")
);

var app = builder.Build();
app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = ["index.html"] });
app.UseStaticFiles();
var baseRoute = app.MapGroup("api");
baseRoute.MapAccounts();
baseRoute.MapTransactions();

// baseRoute.MapGet("categories",
//     async ([FromServices] LedgerDbContext db) => {
//         var results = await db.Transactions
//             .GroupBy(t => t.Category).Select(g => new {
//            Category = g.Key,
//            Amount = g.Sum(t => t.Amount)
//         }).ToListAsync();
//
//         return Results.Ok(results);
//     });

app.Run();

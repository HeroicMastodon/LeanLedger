using System.Reflection;
using System.Text.Json.Serialization;
using LeanLedgerServer.Accounts;
using LeanLedgerServer.Automation;
using LeanLedgerServer.Categories;
using LeanLedgerServer.Common;
using LeanLedgerServer.TransactionImport;
using LeanLedgerServer.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions {
        Args = args,
        WebRootPath = "StaticFiles",
    }
);
var config = builder.Configuration;
builder.Services.AddDbContext<LedgerDbContext>(
    options => options.UseSqlite(config.GetConnectionString("SQLite"))
);
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<Importer>();
builder.Services.AddScoped<RuleService>();
builder.Services.AddControllers()
    .AddJsonOptions(
        options => {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }
    );
;

var app = builder.Build();

if (app.Environment.IsProduction()) {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
    dbContext.Database.Migrate(); // This applies pending migrations
}

app.Use(
    async (context, next) => {
        if (!context.Request.Path.StartsWithSegments("/api") && !Path.HasExtension(context.Request.Path) && context.Request.Path != "/") {
            context.Request.Path = "/index.html";
        }

        await next();
    }
);

app.UseDefaultFiles();
app.UseStaticFiles();
var baseRoute = app.MapGroup("api");
baseRoute.MapAccounts();
baseRoute.MapTransactions();
baseRoute.MapCategories();
baseRoute.MapImport();
baseRoute.MapDelete(
    "admin/transactions/all",
    async ([FromServices] LedgerDbContext dbContext) => {
        var transactionCount = await dbContext.Transactions.IgnoreQueryFilters().CountAsync();
        var deleteCount = await dbContext.Transactions.IgnoreQueryFilters().ExecuteDeleteAsync();

        return Results.Ok(
            new {
                deleteCount,
                transactionCount
            }
        );
    }
);
app.MapControllers();

app.Run();

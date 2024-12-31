using System.Reflection;
using System.Text.Json.Serialization;
using LeanLedgerServer.Accounts;
using LeanLedgerServer.Categories;
using LeanLedgerServer.Common;
using LeanLedgerServer.TransactionImport;
using LeanLedgerServer.Transactions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions {
        Args = args,
        WebRootPath = "StaticFiles"
    }
);
var config = builder.Configuration;
builder.Services.AddDbContext<LedgerDbContext>(
    options => options.UseSqlite(config.GetConnectionString("SQLite"))
);
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<Importer>();
builder.Services.AddControllers()
    .AddJsonOptions(
        options => {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }
    );
;

var app = builder.Build();
app.UseDefaultFiles(
    new DefaultFilesOptions {
        DefaultFileNames = []
    }
);
app.UseStaticFiles();
var baseRoute = app.MapGroup("api");
baseRoute.MapAccounts();
baseRoute.MapTransactions();
baseRoute.MapCategories();
baseRoute.MapImport();
app.MapControllers();

app.Run();

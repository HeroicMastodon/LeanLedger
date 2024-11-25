using LeanLedgerServer.Accounts;
using LeanLedgerServer.Common;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = "StaticFiles" });
builder.Services.AddDbContext<LedgerDbContext>(
    options => options.UseSqlite("Data Source=ledger.db")
);

var app = builder.Build();
app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = ["index.html"] });
app.UseStaticFiles();
var baseRoute = app.MapGroup("/api");
baseRoute.MapAccounts();

app.Run();

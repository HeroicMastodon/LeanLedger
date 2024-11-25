var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, WebRootPath = "StaticFiles" });
var app = builder.Build();

app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = ["index.html"] });
app.UseStaticFiles();


app.Run();
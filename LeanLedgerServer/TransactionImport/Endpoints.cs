namespace LeanLedgerServer.TransactionImport;

public static class Endpoints {
    public static void MapImport(this RouteGroupBuilder routes) {
        var imports = routes.MapGroup("imports");
        var settings = imports.MapGroup("settings");
        settings.MapGet("{id:Guid}", GetImportSettings);
        settings.MapPut("", UpdateImportSettings);
    }

    private static async Task<IResult> GetImportSettings(HttpContext context) {
        // merchants cannot have imports
        // create the import settings if they don't exist yet
        throw new NotImplementedException();
    }

    private static async Task<IResult> UpdateImportSettings(HttpContext context) {
        // merchants cannot have imports
        throw new NotImplementedException();
    }
}

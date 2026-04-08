namespace LeanLedgerServer.PiggyBanks;

using Common;
using Microsoft.AspNetCore.Mvc;
using static Results;

public static class Endpoints {
    public static void MapPiggyBanks(this IEndpointRouteBuilder endpoints) {
        var piggies = endpoints.MapGroup("piggy-banks");

        _ = piggies.MapDelete("{id:guid}", ClosePiggyBank);
    }



    private static async Task<IResult> ClosePiggyBank(Guid id, [FromServices] LedgerDbContext db) {
        var pb = await db.PiggyBanks.FindAsync(id);

        if (pb is null) {
            return NotFound();
        }

        pb.Closed = true;
        _ = db.Update(pb);
        _ = await db.SaveChangesAsync();

        return NoContent();
    }
}


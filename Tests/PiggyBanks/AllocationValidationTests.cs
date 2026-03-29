namespace Tests.PiggyBanks;

using LeanLedgerServer.PiggyBanks;
using LeanLedgerServer.Transactions;
using Microsoft.EntityFrameworkCore;

public class AllocationValidationTests {
    [Test]
    public async Task CannotAddAllocation_ToTransferTransaction() {
        var options = new DbContextOptionsBuilder<LeanLedgerServer.Common.LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LeanLedgerServer.Common.LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var piggy = new PiggyBank { Id = Guid.NewGuid(), Name = "T1" };
        await db.AddAsync(piggy);

        var t = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = DateOnly.FromDateTime(DateTime.Now), Amount = 100m, Type = TransactionType.Transfer };
        await db.AddAsync(t);
        await db.SaveChangesAsync();

        var req = new LeanLedgerServer.Transactions.AllocationRequest(piggy.Id, 10m);

        var result = await LeanLedgerServer.Transactions.AllocationEndpoints.CreateAllocation(t.Id, req, db);

        Assert.That(result is not null);
        Assert.That(result is global::Microsoft.AspNetCore.Http.IResult);
    }

    [Test]
    public async Task CannotAddAllocation_ExceedTransactionAmount() {
        var options = new DbContextOptionsBuilder<LeanLedgerServer.Common.LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LeanLedgerServer.Common.LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var piggy = new PiggyBank { Id = Guid.NewGuid(), Name = "T2" };
        await db.AddAsync(piggy);

        var t = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = DateOnly.FromDateTime(DateTime.Now), Amount = 20m, Type = TransactionType.Expense };
        await db.AddAsync(t);
        await db.AddAsync(new PiggyAllocation { Id = Guid.NewGuid(), PiggyBankId = piggy.Id, TransactionId = t.Id, Amount = 15m });
        await db.SaveChangesAsync();

        var req = new LeanLedgerServer.Transactions.AllocationRequest(piggy.Id, 10m);

        var result = await LeanLedgerServer.Transactions.AllocationEndpoints.CreateAllocation(t.Id, req, db);

        // Expect BadRequest result
        Assert.That(result is global::Microsoft.AspNetCore.Http.IResult);
    }

    [Test]
    public async Task CannotAddAllocation_ToClosedPiggy() {
        var options = new DbContextOptionsBuilder<LeanLedgerServer.Common.LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LeanLedgerServer.Common.LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var piggy = new PiggyBank { Id = Guid.NewGuid(), Name = "T3", Closed = true };
        await db.AddAsync(piggy);

        var t = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = DateOnly.FromDateTime(DateTime.Now), Amount = 100m, Type = TransactionType.Expense };
        await db.AddAsync(t);
        await db.SaveChangesAsync();

        var req = new LeanLedgerServer.Transactions.AllocationRequest(piggy.Id, 10m);

        var result = await LeanLedgerServer.Transactions.AllocationEndpoints.CreateAllocation(t.Id, req, db);

        Assert.That(result is global::Microsoft.AspNetCore.Http.IResult);
    }
}

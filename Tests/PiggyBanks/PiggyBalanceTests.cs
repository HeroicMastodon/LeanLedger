namespace Tests.PiggyBanks;

using LeanLedgerServer.Common;
using LeanLedgerServer.PiggyBanks;
using LeanLedgerServer.Transactions;
using Microsoft.EntityFrameworkCore;

public class PiggyBalanceTests {
    [Test]
    public async Task TestBalance_InitialPlusAllocations_ByMonth() {
        var options = new DbContextOptionsBuilder<LeanLedgerServer.Common.LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LeanLedgerServer.Common.LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var piggy = new PiggyBank { Id = Guid.NewGuid(), Name = "Vacation", InitialBalance = 100m };
        await db.AddAsync(piggy);

        var tx1 = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = new DateOnly(2026, 3, 1), Amount = 50m, Type = TransactionType.Income };
        var tx2 = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = new DateOnly(2026, 4, 1), Amount = 30m, Type = TransactionType.Expense };

        await db.AddAsync(tx1);
        await db.AddAsync(tx2);

        var alloc1 = new PiggyAllocation { Id = Guid.NewGuid(), PiggyBankId = piggy.Id, TransactionId = tx1.Id, Amount = 25m };
        var alloc2 = new PiggyAllocation { Id = Guid.NewGuid(), PiggyBankId = piggy.Id, TransactionId = tx2.Id, Amount = 10m };

        await db.AddAsync(alloc1);
        await db.AddAsync(alloc2);

        await db.SaveChangesAsync();

        var byMarch = new QueryByMonth(3, 2026);

        var allocSumMarch = await db.PiggyAllocations
            .Where(a => a.PiggyBankId == piggy.Id)
            .Join(db.Transactions, a => a.TransactionId, t => t.Id, (a,t) => new { a.Amount, t.Date, t.IsDeleted })
            .Where(x => !x.IsDeleted)
            .Where(x => x.Date.Year < byMarch.Year || (x.Date.Year == byMarch.Year && x.Date.Month <= byMarch.Month))
            .SumAsync(x => x.Amount);

        var balanceMarch = piggy.InitialBalance + allocSumMarch;

        Assert.AreEqual(125m, balanceMarch);

        var byApril = new QueryByMonth(4, 2026);
        var allocSumApril = await db.PiggyAllocations
            .Where(a => a.PiggyBankId == piggy.Id)
            .Join(db.Transactions, a => a.TransactionId, t => t.Id, (a,t) => new { a.Amount, t.Date, t.IsDeleted })
            .Where(x => !x.IsDeleted)
            .Where(x => x.Date.Year < byApril.Year || (x.Date.Year == byApril.Year && x.Date.Month <= byApril.Month))
            .SumAsync(x => x.Amount);

        var balanceApril = piggy.InitialBalance + allocSumApril;
        Assert.AreEqual(135m, balanceApril);
    }
}

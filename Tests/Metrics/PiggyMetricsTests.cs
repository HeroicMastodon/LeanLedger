namespace Tests.Metrics;

using LeanLedgerServer.Common;
using LeanLedgerServer.PiggyBanks;
using LeanLedgerServer.Transactions;
using Microsoft.EntityFrameworkCore;

public class PiggyMetricsTests {
    [Test]
    public async Task Metrics_ReturnsProgressAndTotals() {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        _ = await db.Database.EnsureCreatedAsync();

        var pig1 = new PiggyBank { Id = Guid.NewGuid(), Name = "P1", TargetBalance = 100m, };
        var pig2 = new PiggyBank { Id = Guid.NewGuid(), Name = "P2", };
        _ = await db.AddAsync(pig1);
        _ = await db.AddAsync(pig2);

        var tx = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = new DateOnly(2026, 3, 5), Amount = 30m, Type = TransactionType.Income };
        _ = await db.AddAsync(tx);
        _ = await db.AddAsync(new PiggyBankEntry { Id = Guid.NewGuid(), PiggyBankId = pig1.Id, TransactionId = tx.Id, Amount = 25m });
        _ = await db.SaveChangesAsync();

        var byMonth = new QueryByMonth(3, 2026);

        // compute piggy balances
        var piggies = await db.PiggyBanks
            .Select(pb => new {
                pb.Id,
                pb.Name,
                Target = pb.TargetBalance
            })
            .ToListAsync();

        var results = new List<(decimal Balance, decimal? Target)>();
        foreach (var pb in piggies) {
            var allocSum = await db.PiggyBankEntries
                .Where(a => a.PiggyBankId == pb.Id)
                .Join(db.Transactions, a => a.TransactionId, t => t.Id, (a, t) => new { a.Amount, t.Date, t.IsDeleted })
                .Where(x => !x.IsDeleted)
                .Where(x => x.Date.Year < byMonth.Year || (x.Date.Year == byMonth.Year && x.Date.Month <= byMonth.Month))
                .SumAsync(x => x.Amount);

            // var balance = pb.Initial + allocSum;
            // results.Add((balance, pb.Target));
        }

        var piggyTotal = results.Sum(r => r.Balance);
        Assert.That(piggyTotal, Is.EqualTo(75m));

        var (balance, target) = results.First(r => r.Target is not null);
        Assert.Multiple(() => {
            Assert.That(target, Is.EqualTo(100m));
            Assert.That(balance, Is.EqualTo(25m));
        });
    }
}

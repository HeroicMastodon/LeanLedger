namespace Tests.Automation.RuleAction;

using LeanLedgerServer.Automation;
using LeanLedgerServer.Transactions;
using LeanLedgerServer.PiggyBanks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

public class AddPiggyAllocationTests {
    [Test]
    public async Task RuleAction_AddPiggyAllocation_AppliesAndPersists() {
        var options = new DbContextOptionsBuilder<LeanLedgerServer.Common.LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LeanLedgerServer.Common.LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var piggy = new PiggyBank { Id = Guid.NewGuid(), Name = "R1" };
        await db.AddAsync(piggy);

        var tx = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = new DateOnly(2026, 3, 10), Amount = 100m, Type = TransactionType.Income };
        await db.AddAsync(tx);
        await db.SaveChangesAsync();

        var rule = new Rule {
            Id = Guid.NewGuid(),
            Name = "AddAlloc",
            IsStrict = false,
            Triggers = new List<RuleTrigger>(),
            Actions = new List<RuleAction> {
                new RuleAction(RuleActionType.AddPiggyAllocation, null, null, piggy.Id, 25m)
            }
        };

        await db.AddAsync(rule);
        await db.SaveChangesAsync();

        var service = new RuleService(db);
        var start = tx.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = start;

        var changed = await service.Run(rule, start, end);

        Assert.That(changed, Is.GreaterThanOrEqualTo(1));

        var alloc = await db.PiggyAllocations.FirstOrDefaultAsync(a => a.TransactionId == tx.Id && a.PiggyBankId == piggy.Id);
        Assert.IsNotNull(alloc);
        Assert.AreEqual(25m, alloc.Amount);
    }

    [Test]
    public async Task RuleAction_AddPiggyAllocation_ViolationFails() {
        var options = new DbContextOptionsBuilder<LeanLedgerServer.Common.LedgerDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new LeanLedgerServer.Common.LedgerDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var piggy = new PiggyBank { Id = Guid.NewGuid(), Name = "R2" };
        await db.AddAsync(piggy);

        var tx = new Transaction { Id = Guid.NewGuid(), UniqueHash = "", Date = new DateOnly(2026, 3, 10), Amount = 10m, Type = TransactionType.Income };
        await db.AddAsync(tx);
        await db.SaveChangesAsync();

        var rule = new Rule {
            Id = Guid.NewGuid(),
            Name = "AddAllocTooBig",
            IsStrict = false,
            Triggers = new List<RuleTrigger>(),
            Actions = new List<RuleAction> {
                new RuleAction(RuleActionType.AddPiggyAllocation, null, null, piggy.Id, 25m)
            }
        };

        await db.AddAsync(rule);
        await db.SaveChangesAsync();

        var service = new RuleService(db);
        var start = tx.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = start;

        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Run(rule, start, end));
    }
}

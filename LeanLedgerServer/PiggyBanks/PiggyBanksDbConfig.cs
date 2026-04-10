namespace LeanLedgerServer.PiggyBanks;

using LeanLedgerServer.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PiggyBanksDbConfig: IEntityTypeConfiguration<PiggyBank> {
    public void Configure(EntityTypeBuilder<PiggyBank> builder) {
        _ = builder.HasKey(pb => pb.Id);

        _ = builder.HasMany(pb => pb.Entries)
            .WithOne(a => a.PiggyBank)
            .HasForeignKey(a => a.PiggyBankId);
    }
}

public class PiggyEntryDbConfig: IEntityTypeConfiguration<PiggyBankEntry> {
    public void Configure(EntityTypeBuilder<PiggyBankEntry> builder) {
        _ = builder.HasKey(a => a.Id);

        // TODO: Test that deleting a piggy bank deletes its entries
        _ = builder.HasOne(a => a.PiggyBank)
            .WithMany(pb => pb.Entries)
            .HasForeignKey(a => a.PiggyBankId)
            .IsRequired(true);
    }
}

public static class PiggyFunctions {
    public static IQueryable<PiggyBankMetric> GetPiggyMetrics(
        this LedgerDbContext db,
        QueryByMonth byMonth
    ) {
        var results = db.PiggyBankEntries
            .Include(e => e.PiggyBank)
            .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || ((e.PiggyBank!.OpenDate.Year < byMonth.Year
                        || (e.PiggyBank!.OpenDate.Year == byMonth.Year
                            && e.PiggyBank!.OpenDate.Month <= byMonth.Month
                        )
                     ) && (e.PiggyBank!.CloseDate == null || (
                            e.PiggyBank!.CloseDate.Value.Month >= byMonth.Month
                            && e.PiggyBank!.CloseDate.Value.Year >= byMonth.Year
                    ))
                )
            )
            .GroupBy(e => e.PiggyBank)
            .Select(g => new PiggyBankMetric(
                g.Key!.Id,
                g.Key.Name,
                g.Sum(e => e.Amount),
                g.Key.TargetBalance,
                g.Key.TargetBalance != null
                    ? (g.Sum(e => e.Amount) / g.Key.TargetBalance.Value * 100m)
                    : null
            ));

        return results;
    }
}

public record PiggyBankMetric(
    Guid Id,
    string Name,
    decimal Balance,
    decimal? TargetBalance,
    decimal? Progress
);

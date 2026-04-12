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
        var results = db.PiggyBanks
            .Where(p => byMonth.Month == null
                || byMonth.Year == null
                || ((p.OpenDate.Year < byMonth.Year
                        || (p.OpenDate.Year == byMonth.Year
                            && p.OpenDate.Month <= byMonth.Month))
                    && (p.CloseDate == null
                        || p.CloseDate.Value.Year > byMonth.Year
                        || (p.CloseDate.Value.Year == byMonth.Year
                            && p.CloseDate.Value.Month >= byMonth.Month))))
            .Select(p => new {
                p.Id,
                p.Name,
                Balance = p.Entries
                .Where(e => byMonth.Month == null
                || byMonth.Year == null
                || (
                    e.Date.Year <= byMonth.Year
                    && e.Date.Month <= byMonth.Month
                ))
                .Sum(e => (decimal?)e.Amount) ?? 0m,
                p.TargetBalance
            })
            .Select(x => new PiggyBankMetric(
                x.Id,
                x.Name,
                x.Balance,
                x.TargetBalance,
                x.TargetBalance != null
                    ? (x.Balance / x.TargetBalance.Value * 100m)
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

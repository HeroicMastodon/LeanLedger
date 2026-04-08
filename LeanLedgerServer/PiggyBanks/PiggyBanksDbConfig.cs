namespace LeanLedgerServer.PiggyBanks;

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

        // TODO: thoroughly test that deleting an entry or a piggy bank does not delete transactions
        _ = builder.HasOne(a => a.Transaction)
            .WithMany()
            .HasForeignKey(a => a.TransactionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

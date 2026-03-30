namespace LeanLedgerServer.PiggyBanks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PiggyBanksDbConfig: IEntityTypeConfiguration<PiggyBank> {
    public void Configure(EntityTypeBuilder<PiggyBank> builder) {
        builder.HasKey(pb => pb.Id);

        builder.HasMany(pb => pb.Allocations)
            .WithOne(a => a.PiggyBank)
            .HasForeignKey(a => a.PiggyBankId);
    }
}

public class PiggyAllocationDbConfig: IEntityTypeConfiguration<PiggyAllocation> {
    public void Configure(EntityTypeBuilder<PiggyAllocation> builder) {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.PiggyBank)
            .WithMany(pb => pb.Allocations)
            .HasForeignKey(a => a.PiggyBankId);

        builder.HasOne(a => a.Transaction)
            .WithMany()
            .HasForeignKey(a => a.TransactionId);
    }
}

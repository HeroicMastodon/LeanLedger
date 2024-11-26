using Microsoft.EntityFrameworkCore;

namespace LeanLedgerServer.Common;

using Accounts;
using Transactions;

public class LedgerDbContext(DbContextOptions<LedgerDbContext> options): DbContext(options) {
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Account>(
            entity => {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AccountType) .HasConversion<string>();
                entity.HasQueryFilter(a => !a.IsDeleted);

                entity.HasMany(t => t.Withdrawls)
                    .WithOne(t => t.SourceAccount)
                    .HasForeignKey(t => t.SourceAccountId);

                entity.HasMany(t => t.Deposits)
                    .WithOne(t => t.DestinationAccount)
                    .HasForeignKey(t => t.DestinationAccountId);
            }
        );

        modelBuilder.Entity<Transaction>(
            entity => {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.UniqueHash).IsRequired();
                entity.Property(t => t.Type).HasConversion<string>();

                entity.HasOne(t => t.SourceAccount)
                    .WithMany(a => a.Withdrawls)
                    .HasForeignKey(t => t.SourceAccountId);

                entity.HasOne(t => t.DestinationAccount)
                    .WithMany(a => a.Deposits)
                    .HasForeignKey(t => t.DestinationAccountId);

                entity.HasQueryFilter(t => !t.IsDeleted);
            }
        );
    }
}

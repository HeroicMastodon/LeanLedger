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
            }
        );

        modelBuilder.Entity<Transaction>(
            entity => {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.UniqueHash).IsRequired();
                entity.Property(t => t.Type).HasConversion<string>();

                entity.HasOne(e => e.SourceAccount)
                    .WithMany()
                    .HasForeignKey(e => e.SourceAccountId);

                entity.HasOne(t => t.DestinationAccount)
                    .WithMany()
                    .HasForeignKey(t => t.DestinationAccountId);

                entity.HasQueryFilter(t => !t.IsDeleted);
            }
        );
    }
}

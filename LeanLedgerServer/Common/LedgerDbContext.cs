using Microsoft.EntityFrameworkCore;

namespace LeanLedgerServer.Common;

using Accounts;

public class LedgerDbContext(DbContextOptions<LedgerDbContext> options): DbContext(options) {
    public DbSet<Account> Accounts { get; set; }

    // Optionally configure entity settings using Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Account>(
            entity => {
                entity.HasKey(a => a.Id); // Assuming Name as a unique identifier; otherwise, add an Id property.
                entity.Property(a => a.AccountType)
                    .HasConversion<string>(); // Store enum as string
            }
        );
    }
}

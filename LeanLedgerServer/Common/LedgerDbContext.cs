using Microsoft.EntityFrameworkCore;

namespace LeanLedgerServer.Common;

using System.Text.Json;
using System.Text.Json.Serialization;
using Accounts;
using Automation;
using Budgets;
using TransactionImport;
using Transactions;

public class LedgerDbContext(DbContextOptions<LedgerDbContext> options): DbContext(options) {
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ImportSettings> ImportSettings { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<RuleGroup> RuleGroups { get; set; }
    public DbSet<Budget> Budgets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Account>(
            entity => {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AccountType).HasConversion<string>();
                entity.Property(a => a.IncludeInNetWorth).HasDefaultValue(false);

                entity.HasMany(t => t.Withdrawls)
                    .WithOne(t => t.SourceAccount)
                    .HasForeignKey(t => t.SourceAccountId);
                entity.HasMany(t => t.Deposits)
                    .WithOne(t => t.DestinationAccount)
                    .HasForeignKey(t => t.DestinationAccountId);

                entity.HasQueryFilter(a => !a.IsDeleted);
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

        modelBuilder.Entity<ImportSettings>(
            entity => {
                entity.HasKey(s => s.Id);

                var jsonSerializerSettings = new JsonSerializerOptions {
                   DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                entity.Property(s => s.AttachedAccountId).IsRequired();
                entity.Property(s => s.ImportMappings)
                    .HasConversion(
                        im => JsonSerializer.Serialize(im, jsonSerializerSettings),
                        json => JsonSerializer.Deserialize<List<ImportMapping>>(json, jsonSerializerSettings) ??
                                new List<ImportMapping>()
                    );

                entity.HasOne(s => s.AttachedAccount)
                    .WithOne()
                    .HasForeignKey<ImportSettings>(s => s.AttachedAccountId);

                entity.HasQueryFilter(s => !s.IsDeleted);
            }
        );

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
    }
}

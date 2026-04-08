namespace LeanLedgerServer.Common;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Text.Json.Serialization;
using LeanLedgerServer.Accounts;
using LeanLedgerServer.Automation;
using LeanLedgerServer.Budgets;
using LeanLedgerServer.PiggyBanks;
using LeanLedgerServer.TransactionImport;
using LeanLedgerServer.Transactions;

public class LedgerDbContext(DbContextOptions<LedgerDbContext> options): DbContext(options) {
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ImportSettings> ImportSettings { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<RuleGroup> RuleGroups { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<PiggyBank> PiggyBanks { get; set; }
    public DbSet<PiggyBankEntry> PiggyBankEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        _ = modelBuilder.Entity<Account>(
            entity => {
                _ = entity.HasKey(a => a.Id);
                _ = entity.Property(a => a.AccountType).HasConversion<string>();
                _ = entity.Property(a => a.IncludeInNetWorth).HasDefaultValue(false);

                _ = entity.HasMany(t => t.Withdrawls)
                    .WithOne(t => t.SourceAccount)
                    .HasForeignKey(t => t.SourceAccountId);
                _ = entity.HasMany(t => t.Deposits)
                    .WithOne(t => t.DestinationAccount)
                    .HasForeignKey(t => t.DestinationAccountId);

                _ = entity.HasQueryFilter(a => !a.IsDeleted);
            }
        );

        _ = modelBuilder.Entity<Transaction>(
            entity => {
                _ = entity.HasKey(t => t.Id);

                _ = entity.Property(t => t.UniqueHash).IsRequired();
                _ = entity.Property(t => t.Type).HasConversion<string>();

                _ = entity.HasOne(t => t.SourceAccount)
                    .WithMany(a => a.Withdrawls)
                    .HasForeignKey(t => t.SourceAccountId);

                _ = entity.HasOne(t => t.DestinationAccount)
                    .WithMany(a => a.Deposits)
                    .HasForeignKey(t => t.DestinationAccountId);

                _ = entity.HasQueryFilter(t => !t.IsDeleted);
            }
        );

        _ = modelBuilder.Entity<ImportSettings>(
            entity => {
                _ = entity.HasKey(s => s.Id);

                var jsonSerializerSettings = new JsonSerializerOptions {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                _ = entity.Property(s => s.AttachedAccountId).IsRequired();
                _ = entity.Property(s => s.ImportMappings)
                    .HasConversion(
                        im => JsonSerializer.Serialize(im, jsonSerializerSettings),
                        json => JsonSerializer.Deserialize<List<ImportMapping>>(json, jsonSerializerSettings) ??
                                new List<ImportMapping>()
                    );

                _ = entity.HasOne(s => s.AttachedAccount)
                    .WithOne()
                    .HasForeignKey<ImportSettings>(s => s.AttachedAccountId);

                _ = entity.HasQueryFilter(s => !s.IsDeleted);
            }
        );

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
    }
}

namespace LeanLedgerServer.Budgets;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BudgetsDbConfig: IEntityTypeConfiguration<Budget> {
    public void Configure(EntityTypeBuilder<Budget> builder) {
        builder.HasKey(b => b.Id);

        var jsonSerializerSettings = new JsonSerializerOptions();
        builder.Property(b => b.CategoriesGroups)
            .HasConversion(
                a => JsonSerializer.Serialize(a, jsonSerializerSettings),
                json => JsonSerializer.Deserialize<List<BudgetCategoryGroup>>(
                            json,
                            jsonSerializerSettings
                        )
                        ?? new List<BudgetCategoryGroup>()
            );
    }
}

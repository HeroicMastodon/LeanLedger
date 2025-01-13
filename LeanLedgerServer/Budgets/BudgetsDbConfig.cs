namespace LeanLedgerServer.Budgets;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BudgetsDbConfig: IEntityTypeConfiguration<Budget> {
    public void Configure(EntityTypeBuilder<Budget> builder) {
        builder.HasKey(b => b.Id);
    }
}

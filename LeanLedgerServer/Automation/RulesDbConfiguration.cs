namespace LeanLedgerServer.Automation;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RulesDbConfiguration: IEntityTypeConfiguration<Rule> {
    public void Configure(EntityTypeBuilder<Rule> builder) {
        builder.HasKey(r => r.Id);
        builder.HasOne(r => r.RuleGroup).WithMany(rg => rg.Rules).HasForeignKey(r => r.RuleGroupName).IsRequired(false);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

public class RuleGroupsDbConfiguration: IEntityTypeConfiguration<RuleGroup> {
    public void Configure(EntityTypeBuilder<RuleGroup> builder) {
        builder.HasKey(rg => rg.Name);
        builder.HasMany(rg => rg.Rules).WithOne(r => r.RuleGroup).HasForeignKey(r => r.RuleGroupName).IsRequired(false);
    }
}

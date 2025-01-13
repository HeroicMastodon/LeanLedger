namespace LeanLedgerServer.Automation;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RulesDbConfiguration: IEntityTypeConfiguration<Rule> {
    public void Configure(EntityTypeBuilder<Rule> builder) {
        var jsonSerializerSettings = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        builder.HasKey(r => r.Id);
        builder.HasOne(r => r.RuleGroup).WithMany(rg => rg.Rules).HasForeignKey(r => r.RuleGroupName).IsRequired(false);

        builder.Property(r => r.Triggers)
            .HasConversion(
                t => JsonSerializer.Serialize(t, jsonSerializerSettings),
                json => JsonSerializer.Deserialize<List<RuleTrigger>>(json, jsonSerializerSettings)
                        ?? new List<RuleTrigger>()
            );
        builder.Property(r => r.Actions)
            .HasConversion(
                a => JsonSerializer.Serialize(a, jsonSerializerSettings),
                json => JsonSerializer.Deserialize<List<RuleAction>>(json, jsonSerializerSettings)
                        ?? new List<RuleAction>()
            );

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

public class RuleGroupsDbConfiguration: IEntityTypeConfiguration<RuleGroup> {
    public void Configure(EntityTypeBuilder<RuleGroup> builder) {
        builder.HasKey(rg => rg.Name);
        builder.HasMany(rg => rg.Rules).WithOne(r => r.RuleGroup).HasForeignKey(r => r.RuleGroupName).IsRequired(false);
    }
}

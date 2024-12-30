namespace LeanLedgerServer.Automation;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RulesDbConfiguration: IEntityTypeConfiguration<Rule> {
    public void Configure(EntityTypeBuilder<Rule> builder) { }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanLedgerServer.Migrations;
/// <inheritdoc />
public partial class AddPiggyBanks: Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "PiggyBanks",
            columns: table => new {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                InitialBalance = table.Column<decimal>(type: "TEXT", nullable: false),
                BalanceTarget = table.Column<decimal>(type: "TEXT", nullable: true),
                Closed = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table => {
                table.PrimaryKey("PK_PiggyBanks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PiggyAllocations",
            columns: table => new {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                PiggyBankId = table.Column<Guid>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", nullable: false)
            },
            constraints: table => {
                table.PrimaryKey("PK_PiggyAllocations", x => x.Id);
                table.ForeignKey(
                    name: "FK_PiggyAllocations_PiggyBanks_PiggyBankId",
                    column: x => x.PiggyBankId,
                    principalTable: "PiggyBanks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PiggyAllocations_Transactions_TransactionId",
                    column: x => x.TransactionId,
                    principalTable: "Transactions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PiggyAllocations_PiggyBankId",
            table: "PiggyAllocations",
            column: "PiggyBankId");

        migrationBuilder.CreateIndex(
            name: "IX_PiggyAllocations_TransactionId",
            table: "PiggyAllocations",
            column: "TransactionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable(
            name: "PiggyAllocations");

        migrationBuilder.DropTable(
            name: "PiggyBanks");
    }
}

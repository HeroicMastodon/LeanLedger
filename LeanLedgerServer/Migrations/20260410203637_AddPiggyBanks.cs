using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanLedgerServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPiggyBanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PiggyBanks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OpenDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CloseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    TargetBalance = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiggyBanks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PiggyBankEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PiggyBankId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiggyBankEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiggyBankEntries_PiggyBanks_PiggyBankId",
                        column: x => x.PiggyBankId,
                        principalTable: "PiggyBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PiggyBankEntries_PiggyBankId",
                table: "PiggyBankEntries",
                column: "PiggyBankId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PiggyBankEntries");

            migrationBuilder.DropTable(
                name: "PiggyBanks");
        }
    }
}

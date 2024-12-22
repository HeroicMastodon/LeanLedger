using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanLedgerServer.Migrations
{
    /// <inheritdoc />
    public partial class AddImportDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateImported",
                table: "Transactions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateImported",
                table: "Transactions");
        }
    }
}

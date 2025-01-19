using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanLedgerServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categories",
                table: "Budgets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categories",
                table: "Budgets");
        }
    }
}

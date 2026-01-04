using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuackAssistant.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_UniqueCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UniqueCode",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Transactions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Code",
                table: "Transactions",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Code",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "UniqueCode",
                table: "Transactions",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UniqueCode",
                table: "Transactions",
                column: "UniqueCode",
                unique: true);
        }
    }
}

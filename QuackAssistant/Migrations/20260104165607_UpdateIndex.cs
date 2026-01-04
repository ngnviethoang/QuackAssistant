using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuackAssistant.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueCode",
                table: "Transactions",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionTime",
                table: "Transactions",
                column: "TransactionTime",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UniqueCode",
                table: "Transactions",
                column: "UniqueCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Debts_CreationTime",
                table: "Debts",
                column: "CreationTime",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionTime",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UniqueCode",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Debts_CreationTime",
                table: "Debts");

            migrationBuilder.DropColumn(
                name: "UniqueCode",
                table: "Transactions");
        }
    }
}

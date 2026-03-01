using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualBillsSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_manual_bills_CustomerName",
                table: "manual_bills",
                column: "CustomerName");

            migrationBuilder.CreateIndex(
                name: "IX_manual_bills_Phone",
                table: "manual_bills",
                column: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_manual_bills_CustomerName",
                table: "manual_bills");

            migrationBuilder.DropIndex(
                name: "IX_manual_bills_Phone",
                table: "manual_bills");
        }
    }
}

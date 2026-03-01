using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualBillSellerAddressAndTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerAddress",
                table: "manual_bills",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNameTitle",
                table: "manual_bills",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerNameTitle",
                table: "manual_bills",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SellerAddress", table: "manual_bills");
            migrationBuilder.DropColumn(name: "CustomerNameTitle", table: "manual_bills");
            migrationBuilder.DropColumn(name: "SellerNameTitle", table: "manual_bills");
        }
    }
}

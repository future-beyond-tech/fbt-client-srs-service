using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SRS.Infrastructure.Persistence;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260219101500_AddCustomerPhotoUrlToSale")]
    public class AddCustomerPhotoUrlToSale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerPhotoUrl",
                table: "Sales",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "legacy-photo-unavailable");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerPhotoUrl",
                table: "Sales");
        }
    }
}

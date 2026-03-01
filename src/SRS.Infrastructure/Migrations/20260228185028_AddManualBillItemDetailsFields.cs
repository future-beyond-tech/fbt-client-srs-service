using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualBillItemDetailsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChassisNumber",
                table: "manual_bills",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "manual_bills",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineNumber",
                table: "manual_bills",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "manual_bills",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ChassisNumber", table: "manual_bills");
            migrationBuilder.DropColumn(name: "Color", table: "manual_bills");
            migrationBuilder.DropColumn(name: "EngineNumber", table: "manual_bills");
            migrationBuilder.DropColumn(name: "Notes", table: "manual_bills");
        }
    }
}

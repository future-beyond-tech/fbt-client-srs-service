using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleLegalAcknowledgements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OwnershipTransferAccepted",
                table: "Sales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RcBookReceived",
                table: "Sales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleAcceptedInAsIsCondition",
                table: "Sales",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnershipTransferAccepted",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "RcBookReceived",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "VehicleAcceptedInAsIsCondition",
                table: "Sales");
        }
    }
}

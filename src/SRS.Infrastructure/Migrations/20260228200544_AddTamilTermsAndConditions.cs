using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTamilTermsAndConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TamilTermsAndConditions",
                table: "DeliveryNoteSettings",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TamilTermsAndConditions",
                table: "DeliveryNoteSettings");
        }
    }
}

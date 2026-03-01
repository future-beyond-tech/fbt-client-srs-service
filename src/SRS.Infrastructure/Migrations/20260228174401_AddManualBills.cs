using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualBills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manual_bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillNumber = table.Column<int>(type: "integer", nullable: false),
                    BillType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AmountTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMode = table.Column<int>(type: "integer", nullable: false),
                    CashAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UpiAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinanceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FinanceCompany = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manual_bills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_manual_bills_BillNumber",
                table: "manual_bills",
                column: "BillNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manual_bills");
        }
    }
}

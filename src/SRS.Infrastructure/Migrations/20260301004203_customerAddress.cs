using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class customerAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns CustomerNameTitle, SellerAddress, SellerNameTitle already added by AddManualBillSellerAddressAndTitles (20260301002612).
            // This migration is a no-op to avoid duplicate column errors when running migrations in order.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: columns are removed by AddManualBillSellerAddressAndTitles.Down.
        }
    }
}

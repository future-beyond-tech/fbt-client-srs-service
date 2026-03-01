using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class ManualBillConfiguration : IEntityTypeConfiguration<ManualBill>
{
    public void Configure(EntityTypeBuilder<ManualBill> builder)
    {
        builder.ToTable("manual_bills");
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.BillNumber).IsRequired();
        builder.HasIndex(x => x.BillNumber).IsUnique();
        builder.Property(x => x.BillType).HasMaxLength(20).IsRequired().HasDefaultValue("Manual");
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.CustomerName);
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Phone);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.PhotoUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SellerName).HasMaxLength(200);
        builder.Property(x => x.SellerAddress).HasMaxLength(500);
        builder.Property(x => x.CustomerNameTitle).HasMaxLength(10);
        builder.Property(x => x.SellerNameTitle).HasMaxLength(10);
        builder.Property(x => x.ItemDescription).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ChassisNumber).HasMaxLength(100);
        builder.Property(x => x.EngineNumber).HasMaxLength(100);
        builder.Property(x => x.Color).HasMaxLength(80);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.AmountTotal).HasPrecision(18, 2);
        builder.Property(x => x.CashAmount).HasPrecision(18, 2);
        builder.Property(x => x.UpiAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinanceCompany).HasMaxLength(150);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.InvoicePdfUrl).HasMaxLength(1000);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class SaleConfiguration:IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.Property(s => s.BillNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(s => s.BillNumber)
            .IsUnique();

        builder.HasIndex(s => s.SaleDate);
        builder.HasIndex(s => s.VehicleId)
            .IsUnique();

        builder.HasOne(s => s.Vehicle)
            .WithOne(v => v.Sale)
            .HasForeignKey<Sale>(s => s.VehicleId);

        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId);
        builder.Property(x => x.CashAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.FinanceAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Profit)
            .HasPrecision(18, 2);

        builder.Property(x => x.UpiAmount)
            .HasPrecision(18, 2);


    }
}

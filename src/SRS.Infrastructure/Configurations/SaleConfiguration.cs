using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.BillNumber)
            .IsRequired();

        builder.HasIndex(s => s.BillNumber)
            .IsUnique();

        builder.Property(s => s.CustomerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(s => s.CustomerPhone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CustomerAddress)
            .HasMaxLength(300);

        builder.Property(s => s.CustomerPhotoUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.FinanceCompany)
            .HasMaxLength(150);

        builder.HasIndex(s => s.CustomerPhone);
        builder.HasIndex(s => s.CustomerName);
        builder.HasIndex(s => s.SaleDate);
        builder.HasIndex(s => s.VehicleId)
            .IsUnique();

        builder.HasOne(s => s.Vehicle)
            .WithOne(v => v.Sale)
            .HasForeignKey<Sale>(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.SellerName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.SellerPhone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.SellerAddress)
            .HasMaxLength(300);

        builder.Property(p => p.BuyingCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Expense)
            .HasPrecision(18, 2);

        builder.Property(p => p.PurchaseDate)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasIndex(p => p.VehicleId)
            .IsUnique();

        builder.HasOne(p => p.Vehicle)
            .WithOne(v => v.Purchase)
            .HasForeignKey<Purchase>(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

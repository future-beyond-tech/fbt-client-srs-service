using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class VehicleConfiguration:IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.Property(v => v.Brand)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.RegistrationNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        builder.HasIndex(v => v.Status);
        builder.Property(x => x.BuyingCost)
            .HasPrecision(18, 2);

        builder.Property(x => x.Expense)
            .HasPrecision(18, 2);

        builder.Property(x => x.SellingPrice)
            .HasPrecision(18, 2);


    }
}
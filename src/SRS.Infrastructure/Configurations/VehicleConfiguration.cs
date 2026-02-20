using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.Property(v => v.Id)
            .ValueGeneratedOnAdd();

        builder.Property(v => v.Brand)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.RegistrationNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        builder.HasIndex(v => v.Status);

        builder.Property(v => v.ChassisNumber)
            .HasMaxLength(100);

        builder.Property(v => v.EngineNumber)
            .HasMaxLength(100);

        builder.Property(v => v.Colour)
            .HasMaxLength(50);

        builder.Property(v => v.SellingPrice)
            .HasPrecision(18, 2);

        builder.Property(v => v.CreatedAt)
            .IsRequired();
    }
}

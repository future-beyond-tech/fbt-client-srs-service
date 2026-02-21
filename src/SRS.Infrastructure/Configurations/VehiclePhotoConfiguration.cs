using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class VehiclePhotoConfiguration : IEntityTypeConfiguration<VehiclePhoto>
{
    public void Configure(EntityTypeBuilder<VehiclePhoto> builder)
    {
        builder.Property(vp => vp.Id)
            .ValueGeneratedOnAdd();

        builder.Property(vp => vp.PhotoUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(vp => vp.IsPrimary)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(vp => vp.DisplayOrder)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(vp => vp.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(vp => vp.VehicleId);

        builder.HasOne(vp => vp.Vehicle)
            .WithMany(v => v.Photos)
            .HasForeignKey(vp => vp.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

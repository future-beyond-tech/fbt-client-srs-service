using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class DeliveryNoteSettingsConfiguration : IEntityTypeConfiguration<DeliveryNoteSettings>
{
    public void Configure(EntityTypeBuilder<DeliveryNoteSettings> builder)
    {
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ShopName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ShopAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.GSTNumber)
            .HasMaxLength(50);

        builder.Property(x => x.ContactNumber)
            .HasMaxLength(30);

        builder.Property(x => x.FooterText)
            .HasMaxLength(500);

        builder.Property(x => x.TermsAndConditions)
            .HasMaxLength(2000);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.SignatureLine)
            .HasMaxLength(150);

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_DeliveryNoteSettings_Singleton", "\"Id\" = 1"));
    }
}

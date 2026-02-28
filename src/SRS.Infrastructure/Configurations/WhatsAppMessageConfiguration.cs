using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class WhatsAppMessageConfiguration : IEntityTypeConfiguration<WhatsAppMessage>
{
    public void Configure(EntityTypeBuilder<WhatsAppMessage> builder)
    {
        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(w => w.MediaUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(w => w.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.HasIndex(w => w.CreatedAt);
        builder.HasIndex(w => w.SaleId);
        builder.HasIndex(w => w.CustomerId);

        builder.HasOne(w => w.Sale)
            .WithMany(s => s.WhatsAppMessages)
            .HasForeignKey(w => w.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Customer)
            .WithMany()
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Address)
            .HasMaxLength(300);

        builder.Property(c => c.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasIndex(c => c.Phone);

        builder.HasIndex(c => c.Name);
    }
}

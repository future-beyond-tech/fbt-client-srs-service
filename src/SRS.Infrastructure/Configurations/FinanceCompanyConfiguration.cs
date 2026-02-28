using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class FinanceCompanyConfiguration : IEntityTypeConfiguration<FinanceCompany>
{
    public void Configure(EntityTypeBuilder<FinanceCompany> builder)
    {
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.IsActive);
    }
}

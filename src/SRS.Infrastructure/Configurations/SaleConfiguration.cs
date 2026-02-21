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

        builder.Property(s => s.CustomerId)
            .IsRequired();

        builder.Property(s => s.RcBookReceived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.OwnershipTransferAccepted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.VehicleAcceptedInAsIsCondition)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.FinanceCompany)
            .HasMaxLength(150);

        builder.Property(s => s.WitnessName)
            .HasMaxLength(150);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.InvoicePdfUrl)
            .HasMaxLength(1000);

        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.SaleDate);
        builder.HasIndex(s => s.VehicleId)
            .IsUnique();

        builder.HasOne(s => s.Vehicle)
            .WithOne(v => v.Sale)
            .HasForeignKey<Sale>(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
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

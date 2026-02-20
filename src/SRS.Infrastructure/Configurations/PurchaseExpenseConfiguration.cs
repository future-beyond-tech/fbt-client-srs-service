using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Configurations;

public class PurchaseExpenseConfiguration : IEntityTypeConfiguration<PurchaseExpense>
{
    public void Configure(EntityTypeBuilder<PurchaseExpense> builder)
    {
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ExpenseType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.VehicleId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Vehicle)
            .WithMany(v => v.PurchaseExpenses)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

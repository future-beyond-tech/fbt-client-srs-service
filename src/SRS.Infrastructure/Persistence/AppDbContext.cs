using Microsoft.EntityFrameworkCore;
using SRS.Domain.Entities;

namespace SRS.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseExpense> PurchaseExpenses { get; set; }
    public DbSet<VehiclePhoto> VehiclePhotos { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<FinanceCompany> FinanceCompanies { get; set; }
    public DbSet<DeliveryNoteSettings> DeliveryNoteSettings { get; set; }
    public DbSet<WhatsAppMessage> WhatsAppMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

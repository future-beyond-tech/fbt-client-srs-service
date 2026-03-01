using Microsoft.EntityFrameworkCore;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Tests.Shared;

/// <summary>
/// Deterministic test data for integration tests. Uses dummy values only (no PII).
/// Do not log or assert on these in a way that leaks outside test process.
/// </summary>
public static class TestDataSeeder
{
    /// <summary>Dummy name - do not use for real users.</summary>
    public const string DummyCustomerName = "Test Customer";

    /// <summary>Dummy phone - do not use in production or logs.</summary>
    public const string DummyPhone = "9999999999";

    /// <summary>Dummy address.</summary>
    public const string DummyAddress = "Test Address";

    public static async Task<Customer> EnsureTestCustomerAsync(AppDbContext db, string? phone = null, CancellationToken ct = default)
    {
        var p = phone ?? DummyPhone;
        var existing = await db.Customers.FirstOrDefaultAsync(c => c.Phone == p, ct);
        if (existing != null)
            return existing;
        var customer = new Customer
        {
            Name = DummyCustomerName,
            Phone = p,
            Address = DummyAddress
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);
        return customer;
    }

    public static async Task<Vehicle> EnsureTestVehicleAsync(
        AppDbContext db,
        string registrationNumber,
        decimal sellingPrice = 100_000,
        decimal buyingCost = 80_000,
        decimal expense = 5_000,
        CancellationToken ct = default)
    {
        var existing = await db.Vehicles
            .Include(v => v.Purchase)
            .FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber, ct);
        if (existing != null)
            return existing;
        var vehicle = new Vehicle
        {
            Brand = "TestBrand",
            Model = "TestModel",
            Year = DateTime.UtcNow.Year - 1,
            RegistrationNumber = registrationNumber,
            ChassisNumber = "CHASSIS001",
            EngineNumber = "ENG001",
            Colour = "White",
            SellingPrice = sellingPrice,
            Status = VehicleStatus.Available
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);

        var purchase = new Purchase
        {
            VehicleId = vehicle.Id,
            SellerName = "Test Seller",
            SellerPhone = DummyPhone,
            SellerAddress = DummyAddress,
            BuyingCost = buyingCost,
            Expense = expense,
            PurchaseDate = DateTime.UtcNow.AddDays(-30)
        };
        db.Purchases.Add(purchase);
        await db.SaveChangesAsync(ct);
        return vehicle;
    }

    /// <summary>
    /// Snapshot of common JSON options used by the API (camelCase, etc.) for assertions.
    /// </summary>
    public static System.Text.Json.JsonSerializerOptions GetDefaultJsonOptions()
    {
        return new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
}

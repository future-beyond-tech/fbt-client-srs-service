using SRS.Domain.Enums;

namespace SRS.Domain.Entities;

public class Vehicle
{
    public int Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    public string RegistrationNumber { get; set; } = null!;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? Colour { get; set; }
    public decimal SellingPrice { get; set; }

    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Purchase Purchase { get; set; } = null!;
    public Sale? Sale { get; set; }
    public ICollection<PurchaseExpense> PurchaseExpenses { get; set; } = new List<PurchaseExpense>();
    public ICollection<VehiclePhoto> Photos { get; set; } = new List<VehiclePhoto>();
}

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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Purchase Purchase { get; set; } = null!;
    public Sale? Sale { get; set; }
}

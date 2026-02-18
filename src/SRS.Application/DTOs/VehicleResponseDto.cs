using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class VehicleResponseDto
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    public string RegistrationNumber { get; set; } = null!;
    public string ChassisNumber { get; set; } = null!;
    public string EngineNumber { get; set; } = null!;

    public decimal SellingPrice { get; set; }
    public VehicleStatus Status { get; set; }
    public DateTime PurchaseDate { get; set; }
}

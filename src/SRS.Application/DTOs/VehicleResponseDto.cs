using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class VehicleResponseDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    public string RegistrationNumber { get; set; } = null!;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }

    public decimal SellingPrice { get; set; }
    public VehicleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

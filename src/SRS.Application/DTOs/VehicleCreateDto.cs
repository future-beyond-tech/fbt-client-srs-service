namespace SRS.Application.DTOs;

public class VehicleCreateDto
{
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    public string RegistrationNumber { get; set; } = null!;
    public string ChassisNumber { get; set; } = null!;
    public string EngineNumber { get; set; } = null!;

    public decimal BuyingCost { get; set; }
    public decimal Expense { get; set; }
    public decimal SellingPrice { get; set; }

    public DateTime PurchaseDate { get; set; }
}

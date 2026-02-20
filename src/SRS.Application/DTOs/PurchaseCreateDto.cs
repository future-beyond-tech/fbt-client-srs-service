namespace SRS.Application.DTOs;

public class PurchaseCreateDto
{
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    public string RegistrationNumber { get; set; } = null!;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? Colour { get; set; }
    public decimal SellingPrice { get; set; }

    public string SellerName { get; set; } = null!;
    public string SellerPhone { get; set; } = null!;
    public string? SellerAddress { get; set; }

    public decimal BuyingCost { get; set; }
    public decimal Expense { get; set; }
    public DateTime PurchaseDate { get; set; }
}

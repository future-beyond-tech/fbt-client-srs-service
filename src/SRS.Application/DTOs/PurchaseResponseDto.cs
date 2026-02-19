namespace SRS.Application.DTOs;

public class PurchaseResponseDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }

    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public decimal SellingPrice { get; set; }

    public string SellerName { get; set; } = null!;
    public string SellerPhone { get; set; } = null!;
    public string? SellerAddress { get; set; }

    public decimal BuyingCost { get; set; }
    public decimal Expense { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

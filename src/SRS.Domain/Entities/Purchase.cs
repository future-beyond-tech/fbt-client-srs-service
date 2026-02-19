namespace SRS.Domain.Entities;

public class Purchase
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public string SellerName { get; set; } = null!;
    public string SellerPhone { get; set; } = null!;
    public string? SellerAddress { get; set; }

    public decimal BuyingCost { get; set; }
    public decimal Expense { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

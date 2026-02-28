namespace SRS.Application.DTOs;

public class PurchaseExpenseDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string ExpenseType { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

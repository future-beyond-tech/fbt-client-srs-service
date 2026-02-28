namespace SRS.Application.DTOs;

public class PurchaseExpenseCreateDto
{
    public string ExpenseType { get; set; } = null!;
    public decimal Amount { get; set; }
}

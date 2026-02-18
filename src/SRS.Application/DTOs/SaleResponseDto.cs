namespace SRS.Application.DTOs;

public class SaleResponseDto
{
    public string BillNumber { get; set; } = null!;
    public string Vehicle { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public decimal TotalReceived { get; set; }
    public decimal Profit { get; set; }
    public DateTime SaleDate { get; set; }
}

namespace SRS.Application.DTOs;

public class SaleResponseDto
{
    public int BillNumber { get; set; }
    public int VehicleId { get; set; }
    public string Vehicle { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public decimal TotalReceived { get; set; }
    public decimal Profit { get; set; }
    public DateTime SaleDate { get; set; }
}

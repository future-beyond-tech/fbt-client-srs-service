namespace SRS.Application.DTOs;

public class SaleResponseDto
{
    public int BillNumber { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime? InvoiceGeneratedAt { get; set; }
    public string InvoiceStatus { get; set; } = null!;
    public int VehicleId { get; set; }
    public string Vehicle { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public decimal TotalReceived { get; set; }
    public decimal Profit { get; set; }
    public DateTime SaleDate { get; set; }
}

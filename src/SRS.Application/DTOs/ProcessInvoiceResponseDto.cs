namespace SRS.Application.DTOs;

public class ProcessInvoiceResponseDto
{
    public int BillNumber { get; set; }
    public string PdfUrl { get; set; } = null!;
    public string WhatsAppStatus { get; set; } = null!;
    public DateTime? GeneratedAt { get; set; }
}

namespace SRS.Application.DTOs;

public class SendInvoiceResponseDto
{
    public int BillNumber { get; set; }
    public string PdfUrl { get; set; } = null!;
    public string Status { get; set; } = null!;
}

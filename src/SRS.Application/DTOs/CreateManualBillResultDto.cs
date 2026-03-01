namespace SRS.Application.DTOs;

public class CreateManualBillResultDto
{
    public int BillNumber { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

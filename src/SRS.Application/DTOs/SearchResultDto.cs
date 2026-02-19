namespace SRS.Application.DTOs;

public class SearchResultDto
{
    public int BillNumber { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string Vehicle { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public DateTime SaleDate { get; set; }
}

namespace SRS.Application.DTOs;

public class SearchResultDto
{
    public string BillNumber { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Vehicle { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public DateTime SaleDate { get; set; }
}

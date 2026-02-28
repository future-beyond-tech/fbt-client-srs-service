namespace SRS.Application.DTOs;

public class SaleHistoryDto
{
    public int BillNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;
    public string RegistrationNumber { get; set; } = null!;
    public decimal Profit { get; set; }
}

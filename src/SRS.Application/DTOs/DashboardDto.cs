namespace SRS.Application.DTOs;

public class DashboardDto
{
    public int TotalVehiclesPurchased { get; set; }
    public int TotalVehiclesSold { get; set; }
    public int AvailableVehicles { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal SalesThisMonth { get; set; }
}

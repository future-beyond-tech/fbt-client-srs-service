namespace SRS.Application.DTOs;

public class BillDetailDto
{
    public string BillNumber { get; set; } = null!;
    public DateTime SaleDate { get; set; }

    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public string ChassisNumber { get; set; } = null!;
    public string EngineNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? PhotoUrl { get; set; }

    public decimal CashAmount { get; set; }
    public decimal UpiAmount { get; set; }
    public decimal FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public decimal TotalReceived { get; set; }
}

using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class BillDetailDto
{
    public int BillNumber { get; set; }
    public DateTime SaleDate { get; set; }

    public int VehicleId { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public decimal SellingPrice { get; set; }

    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string? CustomerAddress { get; set; }

    public DateTime PurchaseDate { get; set; }
    public decimal BuyingCost { get; set; }
    public decimal Expense { get; set; }

    public PaymentMode PaymentMode { get; set; }
    public decimal? CashAmount { get; set; }
    public decimal? UpiAmount { get; set; }
    public decimal? FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public decimal Profit { get; set; }

    public decimal TotalReceived { get; set; }
}

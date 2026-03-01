using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class ManualBillCreateDto
{
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public string PhotoUrl { get; set; } = null!;
    public string? SellerName { get; set; }
    public string? SellerAddress { get; set; }
    public string? CustomerNameTitle { get; set; }
    public string? SellerNameTitle { get; set; }
    public string ItemDescription { get; set; } = null!;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? Color { get; set; }
    public string? Notes { get; set; }
    public decimal AmountTotal { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public decimal? CashAmount { get; set; }
    public decimal? UpiAmount { get; set; }
    public decimal? FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }
}

using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class SaleCreateDto
{
    public Guid VehicleId { get; set; }

    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? PhotoUrl { get; set; }

    public PaymentMode PaymentMode { get; set; }
    public decimal CashAmount { get; set; }
    public decimal UpiAmount { get; set; }
    public decimal FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public DateTime SaleDate { get; set; }
}

using SRS.Domain.Enums;

namespace SRS.Application.DTOs;

public class SaleCreateDto
{
    public int VehicleId { get; set; }

    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string CustomerPhotoUrl { get; set; } = null!;

    public PaymentMode PaymentMode { get; set; }
    public decimal? CashAmount { get; set; }
    public decimal? UpiAmount { get; set; }
    public decimal? FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public DateTime SaleDate { get; set; }
}

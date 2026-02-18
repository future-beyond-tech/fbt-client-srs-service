using SRS.Domain.Common;
using SRS.Domain.Enums;

namespace SRS.Domain.Entities;
public class Sale : BaseEntity
{
    public string BillNumber { get; set; } = null!;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public PaymentMode PaymentMode { get; set; }

    public decimal CashAmount { get; set; }
    public decimal UpiAmount { get; set; }
    public decimal FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public DateTime SaleDate { get; set; }

    public decimal Profit { get; set; }
}

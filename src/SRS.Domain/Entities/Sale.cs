using SRS.Domain.Enums;

namespace SRS.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    public int BillNumber { get; set; }

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public PaymentMode PaymentMode { get; set; }

    public decimal? CashAmount { get; set; }
    public decimal? UpiAmount { get; set; }
    public decimal? FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public DateTime SaleDate { get; set; }
    public TimeSpan? DeliveryTime { get; set; }
    public string? WitnessName { get; set; }
    public string? Notes { get; set; }
    public bool RcBookReceived { get; set; } = false;
    public bool OwnershipTransferAccepted { get; set; } = false;
    public bool VehicleAcceptedInAsIsCondition { get; set; } = false;

    public decimal Profit { get; set; }
    public string? InvoicePdfUrl { get; set; }
    public DateTime? InvoiceGeneratedAt { get; set; }

    public ICollection<WhatsAppMessage> WhatsAppMessages { get; set; } = new List<WhatsAppMessage>();
}

using SRS.Domain.Enums;

namespace SRS.Domain.Entities;

/// <summary>
/// Standalone bill not tied to vehicle inventory. Manual entry with customer details and photo.
/// </summary>
public class ManualBill
{
    public int Id { get; set; }
    public int BillNumber { get; set; }

    /// <summary>Bill type identifier, e.g. "Manual".</summary>
    public string BillType { get; set; } = "Manual";

    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public string PhotoUrl { get; set; } = null!;
    /// <summary>Optional custom seller name for the delivery note PDF. When null/empty, shop name from settings is used.</summary>
    public string? SellerName { get; set; }
    /// <summary>Optional seller address for the delivery note PDF. When null/empty, shop address from settings is used.</summary>
    public string? SellerAddress { get; set; }
    /// <summary>Optional title for customer/buyer display, e.g. Mr, Miss, Mrs.</summary>
    public string? CustomerNameTitle { get; set; }
    /// <summary>Optional title for seller display, e.g. Mr, Miss, Mrs.</summary>
    public string? SellerNameTitle { get; set; }
    public string ItemDescription { get; set; } = null!;
    /// <summary>Optional chassis number from manual entry.</summary>
    public string? ChassisNumber { get; set; }
    /// <summary>Optional engine number from manual entry.</summary>
    public string? EngineNumber { get; set; }
    /// <summary>Optional color from manual entry.</summary>
    public string? Color { get; set; }
    /// <summary>Optional notes from manual entry.</summary>
    public string? Notes { get; set; }
    public decimal AmountTotal { get; set; }

    public PaymentMode PaymentMode { get; set; }
    public decimal? CashAmount { get; set; }
    public decimal? UpiAmount { get; set; }
    public decimal? FinanceAmount { get; set; }
    public string? FinanceCompany { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>URL of the generated delivery note PDF (stored in configured cloud storage).</summary>
    public string? InvoicePdfUrl { get; set; }
    public DateTime? InvoiceGeneratedAt { get; set; }
}

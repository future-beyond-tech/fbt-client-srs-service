namespace SRS.Application.DTOs;

/// <summary>
/// Unified search result for global search. Type discriminator: "Sale" | "ManualBill".
/// Phone is masked (last 4 digits visible) to limit PII exposure.
/// </summary>
public class SearchResultDto
{
    /// <summary>Discriminator: "Sale" or "ManualBill".</summary>
    public string Type { get; set; } = null!;

    public int BillNumber { get; set; }
    public string CustomerName { get; set; } = null!;
    /// <summary>Masked phone (e.g. ******3210).</summary>
    public string CustomerPhone { get; set; } = null!;
    /// <summary>Vehicle summary (Sales only). Null for ManualBill.</summary>
    public string? Vehicle { get; set; }
    /// <summary>Registration number (Sales only). Null for ManualBill.</summary>
    public string? RegistrationNumber { get; set; }
    /// <summary>Bill/sale date for ordering and display.</summary>
    public DateTime SaleDate { get; set; }
}

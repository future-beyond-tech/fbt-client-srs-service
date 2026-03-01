namespace SRS.Domain.Entities;

public class DeliveryNoteSettings
{
    public int Id { get; set; }
    public string ShopName { get; set; } = null!;
    public string ShopAddress { get; set; } = null!;
    public string? GSTNumber { get; set; }
    public string? ContactNumber { get; set; }
    public string? FooterText { get; set; }
    public string? TermsAndConditions { get; set; }
    /// <summary>Tamil terms and conditions for Manual Billing PDF (bullet lines). When set, used instead of TermsAndConditions for Tamil block.</summary>
    public string? TamilTermsAndConditions { get; set; }
    public string? LogoUrl { get; set; }
    public string? SignatureLine { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

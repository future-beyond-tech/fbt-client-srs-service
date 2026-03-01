namespace SRS.Application.DTOs;

public class DeliveryNoteSettingsDto
{
    public int Id { get; set; }
    public string ShopName { get; set; } = null!;
    public string ShopAddress { get; set; } = null!;
    public string? ShopTagline { get; set; }
    public string? ShopTagline2 { get; set; }
    public string? GSTNumber { get; set; }
    public string? ContactNumber { get; set; }
    public string? FooterText { get; set; }
    public string? TermsAndConditions { get; set; }
    /// <summary>Tamil terms for PDF (bullet lines, newline-separated). Used for Manual Billing Tamil block when set.</summary>
    public string? TamilTermsAndConditions { get; set; }
    public string? LogoUrl { get; set; }
    public string? SignatureLine { get; set; }
    public DateTime UpdatedAt { get; set; }
}

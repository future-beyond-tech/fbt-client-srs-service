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
    public string? LogoUrl { get; set; }
    public string? SignatureLine { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

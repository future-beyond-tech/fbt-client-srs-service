namespace SRS.Domain.Entities;

public class WhatsAppMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
    public string MediaUrl { get; set; } = null!;
    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

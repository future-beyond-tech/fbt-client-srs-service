namespace SRS.Application.Interfaces;

public interface IWhatsAppService
{
    Task<string> SendInvoiceAsync(
        string toPhoneNumber,
        string customerName,
        string mediaUrl,
        CancellationToken cancellationToken = default);
}

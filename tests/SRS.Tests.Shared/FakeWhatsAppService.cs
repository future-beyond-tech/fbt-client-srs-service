using SRS.Application.Interfaces;

namespace SRS.Tests.Shared;

/// <summary>
/// No-op implementation of <see cref="IWhatsAppService"/> for integration tests.
/// Prevents real Meta API calls; no secrets or PII logged.
/// </summary>
public sealed class FakeWhatsAppService : IWhatsAppService
{
    public Task<string> SendInvoiceAsync(
        string toPhoneNumber,
        string customerName,
        string mediaUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult("OK");
    }
}

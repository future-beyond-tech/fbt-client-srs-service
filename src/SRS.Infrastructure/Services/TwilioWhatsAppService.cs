using Microsoft.Extensions.Configuration;
using SRS.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Collections.Generic;

namespace SRS.Application.Services;

public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly string accountSid;
    private readonly string authToken;
    private readonly string fromNumber;

    public TwilioWhatsAppService(IConfiguration configuration)
    {
        accountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
        authToken = configuration["Twilio:AuthToken"] ?? string.Empty;
        fromNumber = configuration["Twilio:FromNumber"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(accountSid) ||
            string.IsNullOrWhiteSpace(authToken) ||
            string.IsNullOrWhiteSpace(fromNumber))
        {
            throw new InvalidOperationException("Twilio configuration is missing.");
        }
    }

    public async Task<string> SendInvoiceAsync(
        string toPhoneNumber,
        string customerName,
        string mediaUrl,
        CancellationToken cancellationToken = default)
    {
        TwilioClient.Init(accountSid, authToken);

        var message = await MessageResource.CreateAsync(
            from: new PhoneNumber(EnsureWhatsAppAddress(fromNumber)),
            to: new PhoneNumber(EnsureWhatsAppAddress(toPhoneNumber)),
            body: $"Hello {customerName}, your vehicle invoice is attached.",
            mediaUrl: new List<Uri> { new(mediaUrl) });

        cancellationToken.ThrowIfCancellationRequested();

        if (message.ErrorCode is not null)
        {
            throw new InvalidOperationException($"Twilio WhatsApp send failed: {message.ErrorMessage}");
        }

        return message.Status?.ToString() ?? "Queued";
    }

    private static string EnsureWhatsAppAddress(string value)
    {
        if (value.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return $"whatsapp:{value}";
    }
}

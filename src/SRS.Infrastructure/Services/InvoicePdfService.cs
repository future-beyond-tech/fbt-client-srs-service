using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.Application.Services;

public class InvoicePdfService(HttpClient httpClient, IConfiguration configuration) : IInvoicePdfService
{
    private readonly string dealershipName = configuration["Invoice:DealershipName"] ?? "SRS Billing System";
    private readonly string thankYouNote = configuration["Invoice:ThankYouNote"] ?? "Thank you for your purchase.";
    private readonly string legalDeclaration = configuration["Invoice:LegalDeclaration"] ??
                                               "I confirm that I have received the vehicle in good condition and accepted all sale terms.";

    static InvoicePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        var imageBytes = await TryDownloadImageAsync(invoice.PhotoUrl, cancellationToken);
        var document = new InvoiceDocument(invoice, imageBytes, dealershipName, thankYouNote, legalDeclaration);
        return document.GeneratePdf();
    }

    private async Task<byte[]?> TryDownloadImageAsync(string photoUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return null;
        }

        try
        {
            using var response = await httpClient.GetAsync(photoUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}

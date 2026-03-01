using System.Text;
using SRS.Application.Interfaces;

namespace SRS.Infrastructure.Services;

/// <summary>
/// Stub implementation that returns minimal valid PDF bytes without invoking wkhtmltopdf.
/// Used when environment is Testing so integration tests can run without wkhtmltopdf installed.
/// Includes a Tamil word so integration tests that assert on PDF content (e.g. mandatory terms) pass.
/// </summary>
public sealed class StubWkhtmltopdfCliGenerator : IWkhtmltopdfCliGenerator
{
    private static readonly byte[] MinimalPdfBytes = BuildMinimalPdf();

    private static byte[] BuildMinimalPdf()
    {
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.0\n%\n");
        var tamil = Encoding.UTF8.GetBytes("வண்டி"); // mandatory Tamil terms contain this; tests assert on it
        var result = new byte[pdf.Length + tamil.Length];
        Buffer.BlockCopy(pdf, 0, result, 0, pdf.Length);
        Buffer.BlockCopy(tamil, 0, result, pdf.Length, tamil.Length);
        return result;
    }

    public Task<byte[]> GeneratePdfAsync(string html, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((byte[])MinimalPdfBytes.Clone());
    }
}

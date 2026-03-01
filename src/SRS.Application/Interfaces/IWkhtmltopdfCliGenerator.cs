namespace SRS.Application.Interfaces;

/// <summary>
/// Generates PDF from HTML by invoking the wkhtmltopdf CLI.
/// Used for Sales and Manual Billing delivery note PDFs. Requires wkhtmltopdf to be installed.
/// </summary>
public interface IWkhtmltopdfCliGenerator
{
    /// <summary>Renders HTML to PDF using wkhtmltopdf. HTML should include &lt;meta charset="utf-8"&gt; for Tamil/UTF-8.</summary>
    Task<byte[]> GeneratePdfAsync(string html, CancellationToken cancellationToken = default);
}

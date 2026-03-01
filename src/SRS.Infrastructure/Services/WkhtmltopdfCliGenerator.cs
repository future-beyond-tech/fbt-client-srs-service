using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using SRS.Application.Interfaces;

namespace SRS.Infrastructure.Services;

/// <summary>
/// Generates PDF by invoking wkhtmltopdf CLI. No native libwkhtmltox dependency; suitable for Docker.
/// Temp files are written to /tmp (or process temp) and deleted in finally.
/// </summary>
public sealed class WkhtmltopdfCliGenerator(ILogger<WkhtmltopdfCliGenerator> logger) : IWkhtmltopdfCliGenerator
{
    private const string WkhtmltopdfExe = "wkhtmltopdf";

    public async Task<byte[]> GeneratePdfAsync(string html, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(html))
            throw new ArgumentException("HTML content is required.", nameof(html));
        cancellationToken.ThrowIfCancellationRequested();

        string? htmlPath = null;
        string? pdfPath = null;
        try
        {
            var guid = Guid.NewGuid().ToString("N")[..8];
            var tempDir = Path.GetTempPath();
            htmlPath = Path.Combine(tempDir, $"invoice-{guid}.html");
            pdfPath = Path.Combine(tempDir, $"invoice-{guid}.pdf");

            await File.WriteAllTextAsync(htmlPath, html, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

            var args = new[]
            {
                "--encoding", "utf-8",
                "--enable-local-file-access",
                "--print-media-type",
                "--quiet",
                htmlPath,
                pdfPath
            };

            var exitCode = await RunWkhtmltopdfAsync(args, cancellationToken).ConfigureAwait(false);
            if (exitCode != 0)
            {
                logger.LogError("wkhtmltopdf exited with code {ExitCode} for {HtmlPath}.", exitCode, htmlPath);
                throw new InvalidOperationException($"wkhtmltopdf failed with exit code {exitCode}. See logs.");
            }

            if (!File.Exists(pdfPath))
            {
                logger.LogError("wkhtmltopdf did not produce output file {PdfPath}.", pdfPath);
                throw new InvalidOperationException("PDF output file was not created.");
            }

            var bytes = await File.ReadAllBytesAsync(pdfPath, cancellationToken).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                logger.LogError("wkhtmltopdf produced empty PDF for {HtmlPath}.", htmlPath);
                throw new InvalidOperationException("PDF generation returned no data.");
            }

            return bytes;
        }
        finally
        {
            try
            {
                if (htmlPath != null && File.Exists(htmlPath))
                    File.Delete(htmlPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete temp HTML file {Path}.", htmlPath);
            }

            try
            {
                if (pdfPath != null && File.Exists(pdfPath))
                    File.Delete(pdfPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete temp PDF file {Path}.", pdfPath);
            }
        }
    }

    private static async Task<int> RunWkhtmltopdfAsync(string[] args, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = WkhtmltopdfExe,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var a in args)
            psi.ArgumentList.Add(a);

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);

        return process.ExitCode;
    }
}

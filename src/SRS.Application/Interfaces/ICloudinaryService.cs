namespace SRS.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadPdfAsync(byte[] pdfBytes, string fileName, CancellationToken cancellationToken = default);
}

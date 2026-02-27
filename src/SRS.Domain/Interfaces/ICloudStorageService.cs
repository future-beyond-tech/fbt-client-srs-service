namespace SRS.Domain.Interfaces;

public interface ICloudStorageService
{
    Task<string> UploadPdfAsync(byte[] fileBytes, string fileName, CancellationToken ct = default);
}

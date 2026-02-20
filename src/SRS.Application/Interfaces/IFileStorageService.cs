namespace SRS.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);
}

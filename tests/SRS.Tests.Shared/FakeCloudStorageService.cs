using System.Collections.Generic;
using System.Linq;
using System.Text;
using SRS.Domain.Interfaces;

namespace SRS.Tests.Shared;

/// <summary>
/// Fake implementation of <see cref="ICloudStorageService"/> for integration tests.
/// Records upload calls and returns a deterministic HTTPS URL. No secrets; CI-safe.
/// </summary>
public sealed class FakeCloudStorageService : ICloudStorageService
{
    private const string BaseUrl = "https://cdn.test/invoices";

    private readonly List<UploadCall> _calls = new();
    private readonly object _lock = new();

    public IReadOnlyList<UploadCall> UploadCalls
    {
        get { lock (_lock) return _calls.ToList(); }
    }

    public void Reset()
    {
        lock (_lock) _calls.Clear();
    }

    public Task<string> UploadPdfAsync(byte[] fileBytes, string fileName, CancellationToken ct = default)
    {
        if (fileBytes is null || fileName is null)
            throw new ArgumentNullException(fileBytes is null ? nameof(fileBytes) : nameof(fileName));

        var publicId = System.IO.Path.GetFileNameWithoutExtension(fileName) ?? fileName;
        var url = $"{BaseUrl}/{publicId}.pdf";

        lock (_lock)
        {
            _calls.Add(new UploadCall(fileName, fileBytes));
        }

        return Task.FromResult(url);
    }

    /// <summary>Single upload invocation record. Bytes are not logged or exposed as PII.</summary>
    public sealed record UploadCall(string FileName, byte[] Bytes)
    {
        public bool BytesStartWithPdfHeader =>
            Bytes is { Length: >= 4 } && Encoding.ASCII.GetString(Bytes.AsSpan(0, 4)) == "%PDF";

        public bool FileNameContains(string substring) =>
            FileName.Contains(substring, StringComparison.OrdinalIgnoreCase);
    }
}

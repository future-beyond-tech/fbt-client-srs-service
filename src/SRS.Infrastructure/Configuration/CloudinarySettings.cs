namespace SRS.Infrastructure.Configuration;

public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;

    /// <summary>Optional folder for PDF uploads (e.g. "invoices"). Defaults to "invoices" when empty.</summary>
    public string? Folder { get; init; }
}

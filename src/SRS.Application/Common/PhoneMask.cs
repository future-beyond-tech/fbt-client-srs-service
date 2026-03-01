namespace SRS.Application.Common;

/// <summary>
/// Masks PII in logs. Use for phone numbers (e.g. show only last 4 digits).
/// </summary>
public static class PhoneMask
{
    private const int VisibleSuffixLength = 4;
    private const char MaskChar = '*';

    /// <summary>Returns a masked string suitable for logging, e.g. "******3210".</summary>
    public static string MaskLastFour(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return "****";

        var s = phoneNumber.Trim();
        if (s.Length <= VisibleSuffixLength)
            return new string(MaskChar, s.Length);

        var visible = s[^VisibleSuffixLength..];
        var masked = new string(MaskChar, s.Length - VisibleSuffixLength);
        return masked + visible;
    }
}

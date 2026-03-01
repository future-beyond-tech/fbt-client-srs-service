namespace SRS.Application.Common;

/// <summary>
/// Normalizes phone numbers to E.164 (e.g. +91xxxxxxxxxx) for WhatsApp.
/// Does not log or persist raw input (PII).
/// </summary>
public static class PhoneNormalizer
{
    /// <summary>E.164: + followed by 7-15 digits.</summary>
    public static readonly System.Text.RegularExpressions.Regex E164Regex =
        new(@"^\+[1-9]\d{7,14}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Normalizes to E.164. Indian 10-digit becomes +91xxxxxxxxxx.
    /// Strips spaces, dashes, and optional "whatsapp:" prefix.
    /// </summary>
    /// <exception cref="ArgumentException">When input is null/empty or result is not valid E.164.</exception>
    public static string NormalizeToE164(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.");

        var s = phoneNumber.Trim();
        if (s.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            s = s["whatsapp:".Length..].Trim();
        s = s.Replace(" ", string.Empty).Replace("-", string.Empty);

        if (!s.StartsWith('+'))
            s = s.Length == 10 ? $"+91{s}" : $"+{s}";

        if (!E164Regex.IsMatch(s))
            throw new ArgumentException("Phone must be in E.164 format (e.g. +919876543210).");

        return s;
    }
}

namespace SRS.Application.Constants;

/// <summary>
/// PDF content constants for Delivery Note / invoice. Single source of truth for mandatory Tamil terms.
/// </summary>
public static class PdfContentConstants
{
    /// <summary>
    /// Mandatory Tamil terms lines shown on every Sales and Manual Billing invoice. Always rendered; no condition.
    /// Each line includes the checkbox symbol (☑). Do not hardcode these in the HTML builder; use this list.
    /// </summary>
    public static readonly IReadOnlyList<string> TamilTerms =
    [
        "☑ இந்த வண்டியை எந்தவொரு தவறுகளும் இன்றி வாங்க சம்மதிக்கிறேன்.",
        "☑ இந்த வண்டியில் ரிப்பெயர் செலவு ஏதேனும் வந்தாலும் நிர்வாகம் பொறுப்பல்ல.",
        "☑ வண்டியில் ஏதேனும் சேதம் அல்லது எந்தவித காரணத்தால் கோளாறு ஏற்பட்டாலும் இதற்குப் தொடர்பு கொண்டவர்கள் பொறுப்பல்ல.",
        "☑ வண்டி வாங்கிய 15 நாட்களுக்குள் பெயர் மாற்றிக் கொள்ள வேண்டும்."
    ];

    /// <summary>
    /// Default Tamil terms (newline-separated). Used when both TamilTermsAndConditions and TermsAndConditions are empty in settings.
    /// Derived from <see cref="TamilTerms"/> so one source of truth.
    /// </summary>
    public static readonly string DefaultTamilTerms = string.Join("\n", TamilTerms);
}

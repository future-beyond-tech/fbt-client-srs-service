namespace SRS.Application.Common;

/// <summary>
/// Converts numeric amount to words for Indian Rupees (e.g. "One Lakh Twenty Thousand Rupees Only").
/// Used for manual bill PDF; no PDF-layer logic.
/// </summary>
public static class NumberToWordsConverter
{
    private static readonly string[] Units =
    [
        "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
        "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    ];

    private static readonly string[] Tens =
    [
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    ];

    /// <summary>
    /// Converts amount to words in Indian numbering (Lakh, Crore).
    /// Example: 120000 -> "One Lakh Twenty Thousand Rupees Only"
    /// </summary>
    /// <param name="amount">Non-negative amount (decimal part truncated).</param>
    /// <returns>Amount in words ending with "Rupees Only".</returns>
    public static string ToRupeesInWords(decimal amount)
    {
        var n = (long)Math.Floor(Math.Abs(amount));
        if (n == 0)
            return "Zero Rupees Only";

        var words = ToWordsIndian(n);
        return $"{words} Rupees Only";
    }

    /// <summary>
    /// Converts a non-negative integer to words using Indian place values (Lakh, Crore).
    /// </summary>
    public static string ToWordsIndian(long n)
    {
        if (n == 0)
            return "Zero";

        const long crore = 10_000_000;
        const long lakh = 100_000;
        const long thousand = 1_000;

        var parts = new List<string>();

        if (n >= crore)
        {
            parts.Add(ToWordsUnder1000((int)(n / crore)));
            parts.Add("Crore");
            n %= crore;
        }

        if (n >= lakh)
        {
            var l = (int)(n / lakh);
            if (l > 0)
            {
                parts.Add(ToWordsUnder1000(l));
                parts.Add("Lakh");
            }
            n %= lakh;
        }

        if (n >= thousand)
        {
            var t = (int)(n / thousand);
            if (t > 0)
            {
                parts.Add(ToWordsUnder1000(t));
                parts.Add("Thousand");
            }
            n %= thousand;
        }

        if (n > 0)
            parts.Add(ToWordsUnder1000((int)n));

        return string.Join(" ", parts.Where(s => s.Length > 0));
    }

    /// <summary>
    /// Formats amount as Indian currency string (e.g. 120000 -> "1,20,000").
    /// </summary>
    public static string FormatIndianCurrency(decimal amount)
    {
        var n = (long)Math.Floor(Math.Abs(amount));
        return n.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-IN"));
    }

    private static string ToWordsUnder1000(int n)
    {
        if (n == 0)
            return string.Empty;
        if (n < 20)
            return Units[n];
        if (n < 100)
            return Tens[n / 10] + (n % 10 > 0 ? " " + Units[n % 10] : "");
        var h = n / 100;
        var rest = n % 100;
        return Units[h] + " Hundred" + (rest > 0 ? " " + ToWordsUnder1000(rest) : "");
    }
}

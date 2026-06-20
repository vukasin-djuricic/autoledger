using System.Globalization;

namespace AutoLedger.Web.Services;

/// <summary>Formats monetary amounts the way the UI expects: € with de-DE grouping (€48.920,00).</summary>
public static class Money
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-DE");

    public static string Format(decimal amount) => "€" + amount.ToString("N2", Culture);

    /// <summary>Signed amount with a leading − for negatives, e.g. "−€126.500,00".</summary>
    public static string FormatSigned(decimal amount)
    {
        var sign = amount < 0 ? "−" : "";
        return sign + "€" + Math.Abs(amount).ToString("N2", Culture);
    }

    public static string Percent(double value) =>
        value.ToString("0.0", Culture).Replace('.', ',') + "%";
}

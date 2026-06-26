using System.Globalization;

namespace Nexus.Web.Tenant.Services;

/// <summary>
/// Vietnamese money formatting: '.' thousands separator, ',' decimal separator.
/// </summary>
public static class VnMoney
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    public static string Format(decimal value, int decimalPlaces = 0)
    {
        var format = decimalPlaces > 0
            ? $"#,##0.{new string('0', decimalPlaces)}"
            : "#,##0";
        return value.ToString(format, ViCulture);
    }

    public static string FormatNullable(decimal? value, int decimalPlaces = 0) =>
        value.HasValue ? Format(value.Value, decimalPlaces) : "";

    public static bool TryParse(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return true;
        }

        var normalized = input.Trim()
            .Replace(".", "", StringComparison.Ordinal)
            .Replace(",", ".", StringComparison.Ordinal);

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    public static decimal ParseOrZero(string? input) =>
        TryParse(input, out var value) ? value : 0;
}

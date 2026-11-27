using System.Globalization;

namespace PaquitoBot.Application.Common.Frappe;

public static class FrappeTimestampOrder
{
    public static DateTime ParseOrMinimum(string? value)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timestamp)
            ? timestamp
            : DateTime.MinValue;
    }
}

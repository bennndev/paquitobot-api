using System.Globalization;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public static class FrappeFormats
{
    public static DateTime? ParseTimestamp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }

    public static TimeOnly ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("The time value is empty.");
        }

        return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
    }
}

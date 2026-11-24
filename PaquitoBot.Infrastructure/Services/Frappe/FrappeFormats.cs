using System.Globalization;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>
/// Frappe serializes timestamps as "yyyy-MM-dd HH:mm:ss.ffffff" (space separator, no zone),
/// which the default System.Text.Json DateTime converter rejects.
/// </summary>
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

    /// <summary>Frappe may return times without zero padding, for example "9:00:00".</summary>
    public static TimeOnly ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("The time value is empty.");
        }

        return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
    }
}

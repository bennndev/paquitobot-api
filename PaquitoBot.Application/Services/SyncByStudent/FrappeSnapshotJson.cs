using System.Globalization;
using System.Text.Json;

namespace PaquitoBot.Application.Services.SyncByStudent;

internal static class FrappeSnapshotJson
{
    internal static IReadOnlyList<JsonElement> Rows(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray().ToArray();
    }

    internal static string Required(JsonElement row, string property)
    {
        var value = Optional(row, property);
        if (value is null)
        {
            throw new InvalidOperationException($"Frappe field {property} is missing.");
        }

        return value;
    }

    internal static string? Optional(JsonElement row, string property)
    {
        if (!row.TryGetProperty(property, out var value) ||
            value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ToString();
    }

    internal static void EnsureStudent(JsonElement row, string studentName)
    {
        var owner = Optional(row, "student");
        if (owner is not null && !string.Equals(owner, studentName, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Frappe returned another student's records.");
        }
    }

    internal static DateOnly? Date(JsonElement row, string property)
    {
        var value = Optional(row, property);
        if (value is null || value.Length < 10)
        {
            return null;
        }

        if (DateOnly.TryParseExact(value[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
        {
            return date;
        }

        return null;
    }

    internal static TimeOnly Time(JsonElement row, string property)
    {
        if (TimeOnly.TryParse(Optional(row, property), CultureInfo.InvariantCulture, out var time))
        {
            return time;
        }

        throw new InvalidOperationException($"Frappe time {property} is invalid.");
    }

    internal static decimal? Number(JsonElement row, string property)
    {
        if (decimal.TryParse(Optional(row, property), NumberStyles.Number, CultureInfo.InvariantCulture,
                out var number))
        {
            return number;
        }

        return null;
    }

    internal static int? Integer(JsonElement row, string property)
    {
        if (int.TryParse(Optional(row, property), out var number))
        {
            return number;
        }

        return null;
    }

    internal static string Join(params string?[] parts)
    {
        var nonEmptyParts = parts.Where(part => !string.IsNullOrWhiteSpace(part));
        return string.Join(" ", nonEmptyParts);
    }
}

using System.Globalization;
using System.Text.Json;
using PaquitoBot.Application.Integrations.Frappe;

namespace PaquitoBot.Infrastructure.Integrations.Frappe;

internal static class FrappeRequest
{
    public static string ResourcePath(FrappeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(query.Resource);
        if (query.Resource is "." or ".." || query.Resource.Contains('/') || query.Resource.Contains('\\'))
        {
            throw new ArgumentException("The Frappe resource must be a single path segment.", nameof(query));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(query.Offset);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(query.PageSize);

        var parameters = new List<string>
        {
            $"limit_start={query.Offset.ToString(CultureInfo.InvariantCulture)}",
            $"limit_page_length={query.PageSize.ToString(CultureInfo.InvariantCulture)}",
            "order_by=name%20asc"
        };

        if (query.Fields is { Count: > 0 })
        {
            parameters.Add($"fields={Uri.EscapeDataString(JsonSerializer.Serialize(query.Fields))}");
        }

        if (query.Filters is { Count: > 0 })
        {
            var filters = query.Filters.Select(filter => new[] { filter.Field, filter.Operator, filter.Value });
            parameters.Add($"filters={Uri.EscapeDataString(JsonSerializer.Serialize(filters))}");
        }

        if (!string.IsNullOrWhiteSpace(query.ParentDocType))
        {
            parameters.Add($"parent={Uri.EscapeDataString(query.ParentDocType)}");
        }

        return $"api/resource/{Uri.EscapeDataString(query.Resource)}?{string.Join('&', parameters)}";
    }
}

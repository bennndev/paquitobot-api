namespace PaquitoBot.Application.Common.Text;

public static class TextNormalizer
{
    public static string Required(string value)
    {
        return value.Trim();
    }

    public static string RequiredEmail(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    public static string? Optional(string? value)
    {
        var trimmed = value?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static string? OptionalEmail(string? value)
    {
        return Optional(value)?.ToLowerInvariant();
    }
}

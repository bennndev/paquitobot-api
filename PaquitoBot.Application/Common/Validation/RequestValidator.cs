using System.Net.Mail;
using PaquitoBot.Application.Common.Exceptions;

namespace PaquitoBot.Application.Common.Validation;

public static class RequestValidator
{
    public static string RequireText(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new InvalidRequestException($"{field} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new InvalidRequestException($"{field} must not exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static string? OptionalText(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new InvalidRequestException($"{field} must not exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static string RequireEmail(string? value, string field, int maxLength = 254)
    {
        var email = RequireText(value, field, maxLength);

        if (!IsValidEmail(email))
        {
            throw new InvalidRequestException($"{field} is not a valid email address.");
        }

        return email.ToLowerInvariant();
    }

    public static string? OptionalEmail(string? value, string field, int maxLength = 254)
    {
        var email = OptionalText(value, field, maxLength);

        if (email is null)
        {
            return null;
        }

        if (!IsValidEmail(email))
        {
            throw new InvalidRequestException($"{field} is not a valid email address.");
        }

        return email.ToLowerInvariant();
    }

    public static Guid RequireId(Guid value, string field)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidRequestException($"{field} is required.");
        }

        return value;
    }

    private static bool IsValidEmail(string email)
    {
        return MailAddress.TryCreate(email, out var address) && address.Address == email;
    }
}

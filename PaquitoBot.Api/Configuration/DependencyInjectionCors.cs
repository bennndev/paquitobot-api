namespace PaquitoBot.Api;

public static class DependencyInjectionCors
{
    public const string PolicyName = "FrontendOrigins";

    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = (configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [])
            .Select(NormalizeOrigin)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

        return services;
    }

    private static string NormalizeOrigin(string origin)
    {
        var isPlainHttpOrigin = Uri.TryCreate(origin.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.AbsolutePath == "/"
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment);

        if (!isPlainHttpOrigin)
        {
            throw new InvalidOperationException(
                $"{AllowedOriginsKey} entries must be plain http(s) origins such as https://app.example.com, but got '{origin}'.");
        }

        return uri!.GetLeftPart(UriPartial.Authority);
    }
}

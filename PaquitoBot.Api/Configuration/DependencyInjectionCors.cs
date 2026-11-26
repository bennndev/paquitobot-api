namespace PaquitoBot.Api;

public static class DependencyInjectionCors
{
    private const string SectionName = "Cors:AllowedOrigins";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = (configuration.GetSection(SectionName).Get<string[]>() ?? [])
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => origin.Length > 0)
            .ToArray();

        foreach (var origin in origins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            {
                throw new InvalidOperationException(
                    $"{SectionName} contains an invalid origin '{origin}'. Use absolute http(s) origins such as https://example.com.");
            }
        }

        services.AddCors(options =>
            options.AddDefaultPolicy(policy => policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }

    public static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseCors();
        return app;
    }
}

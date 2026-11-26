using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaquitoBot.Application.Integrations.Frappe;

namespace PaquitoBot.Infrastructure.Integrations.Frappe;

public static class FrappeDependencyInjection
{
    public static IHttpClientBuilder AddFrappeIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FrappeOptions>()
            .Bind(configuration.GetSection(FrappeOptions.SectionName))
            .Configure(options =>
            {
                options.ApiKey = configuration["FrappeOAuth:ApiKey"] ?? string.Empty;
                options.ApiSecret = configuration["FrappeOAuth:ApiSecret"] ?? string.Empty;
            })
            .Validate(options => IsCredential(options.ApiKey), "Frappe API key inválida.")
            .Validate(options => IsCredential(options.ApiSecret), "Frappe API secret inválida.")
            .ValidateOnStart();

        return services.AddHttpClient<IFrappeClient, FrappeClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<FrappeOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.Trim().TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("token", $"{options.ApiKey}:{options.ApiSecret}");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false
            })
            .RemoveAllLoggers();
    }

    private static bool IsHttpBaseUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme is "http" or "https" &&
        !string.IsNullOrEmpty(uri.Host) &&
        string.IsNullOrEmpty(uri.UserInfo) &&
        string.IsNullOrEmpty(uri.Query) &&
        string.IsNullOrEmpty(uri.Fragment);

    private static bool IsCredential(string value) =>
        !string.IsNullOrEmpty(value) && value.All(character => character is > ' ' and <= '~' and not ':');
}

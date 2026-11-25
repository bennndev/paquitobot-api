using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>
/// Thin HTTP gateway to the Frappe REST API. Owns base address, authentication and error translation,
/// so each document source only declares its doctype, fields and mapping.
/// </summary>
public sealed class FrappeApiClient
{
    private const int MaxErrorDetailLength = 300;

    private readonly HttpClient _httpClient;

    public FrappeApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        var baseUrl = configuration["Frappe:BaseUrl"];

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException(
                "Configure 'Frappe:BaseUrl' with an absolute URL before using the Frappe client.");
        }

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(baseUri.ToString().TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var apiKey = configuration["Frappe:ApiKey"];
        var apiSecret = configuration["Frappe:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("token", $"{apiKey}:{apiSecret}");
        }
    }

    public async Task<IReadOnlyList<T>> GetListAsync<T>(
        string doctype,
        IReadOnlyCollection<string> fields,
        CancellationToken cancellationToken = default)
    {
        var encodedFields = Uri.EscapeDataString(JsonSerializer.Serialize(fields));
        var endpoint = $"api/resource/{Uri.EscapeDataString(doctype)}?fields={encodedFields}&limit_page_length=0";

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<FrappeResponse<List<T>>>(
            cancellationToken: cancellationToken);

        return payload?.Data ?? [];
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync(cancellationToken);

        if (details.Length > MaxErrorDetailLength)
        {
            details = details[..MaxErrorDetailLength];
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new FrappeAuthenticationException((int)response.StatusCode, details);
        }

        throw new FrappeApiException((int)response.StatusCode, response.ReasonPhrase, details);
    }

    private sealed record FrappeResponse<T>([property: JsonPropertyName("data")] T? Data);
}

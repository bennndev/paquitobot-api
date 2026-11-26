using PaquitoBot.Infrastructure.Configuration;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Application.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeClient(HttpClient httpClient, IOptions<FrappeOptions> options) : IFrappeClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<FrappePage<T>> GetPageAsync<T>(FrappeQuery query, CancellationToken cancellationToken = default)
        where T : notnull
    {
        var response = await GetAsync<ResourceResponse<T>>(FrappeRequest.ResourcePath(query), cancellationToken);
        if (response.Data is not { } items || items.Count > query.PageSize ||
            items.Count > int.MaxValue - query.Offset || items.Any(item => item is null))
        {
            throw new FrappeException(FrappeFailure.InvalidResponse);
        }

        int? nextOffset = items.Count == 0 ? null : query.Offset + items.Count;
        return new FrappePage<T>(items.AsReadOnly(), nextOffset);
    }

    public async Task CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<HealthResponse>("api/method/frappe.auth.get_logged_user", cancellationToken);
        if (string.IsNullOrWhiteSpace(response.Message))
        {
            throw new FrappeException(FrappeFailure.InvalidResponse);
        }

        if (string.Equals(response.Message, "Guest", StringComparison.OrdinalIgnoreCase))
        {
            throw new FrappeException(FrappeFailure.Authentication);
        }
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken) where T : class
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(options.Value.TimeoutSeconds));

        try
        {
            using var response = await httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw new FrappeException(MapFailure(response.StatusCode));
            }

            return await response.Content.ReadFromJsonAsync<T>(SerializerOptions, deadline.Token)
                ?? throw new FrappeException(FrappeFailure.InvalidResponse);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FrappeException(FrappeFailure.Timeout);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException)
        {
            throw new FrappeException(FrappeFailure.Unavailable);
        }
        catch (JsonException)
        {
            throw new FrappeException(FrappeFailure.InvalidResponse);
        }
    }

    private static FrappeFailure MapFailure(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => FrappeFailure.Authentication,
        HttpStatusCode.NotFound => FrappeFailure.NotFound,
        HttpStatusCode.TooManyRequests => FrappeFailure.RateLimited,
        HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => FrappeFailure.Timeout,
        _ when (int)statusCode >= 500 => FrappeFailure.Unavailable,
        _ => FrappeFailure.RequestRejected
    };

    private sealed record ResourceResponse<T>(List<T>? Data);
    private sealed record HealthResponse(string? Message);
}

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services;

public sealed class FrappeStudentSyncService(HttpClient httpClient, IConfiguration configuration)
    : IFrappeStudentSyncService
{
    public async Task<JsonElement> FetchSnapshotAsync(string accessToken, CancellationToken cancellationToken)
    {
        var configuredBaseUrl = configuration["FrappeOAuth:BaseUrl"];
        if (string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            throw new InvalidOperationException("FrappeOAuth:BaseUrl is not configured.");
        }

        var baseUrl = configuredBaseUrl.TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/api/method/paquitobot_student_snapshot");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Frappe student snapshot failed ({(int)response.StatusCode}).");
        }

        await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
        var message = document.RootElement.GetProperty("message");
        var snapshot = message.GetProperty("snapshot");
        return snapshot.Clone();
    }
}

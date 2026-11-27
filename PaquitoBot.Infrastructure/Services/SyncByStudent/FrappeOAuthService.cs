using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.SyncByStudent;

namespace PaquitoBot.Infrastructure.Services;

public sealed class FrappeOAuthService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
    : IFrappeOAuthService
{
    private sealed record PendingAuthorization(Guid UserId, string Email, string CodeVerifier);

    private readonly string _baseUrl = Required(configuration, "FrappeOAuth:BaseUrl").TrimEnd('/');
    private readonly string _clientId = Required(configuration, "FrappeOAuth:ClientId");
    private readonly string _clientSecret = Required(configuration, "FrappeOAuth:ClientSecret");
    private readonly string _redirectUri = Required(configuration, "FrappeOAuth:RedirectUri");

    public FrappeAuthorization Begin(Guid userId, string email)
    {
        var state = RandomBase64Url(32);
        var verifier = RandomBase64Url(32);
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        cache.Set($"frappe-oauth:{state}", new PendingAuthorization(userId, email, verifier),
            TimeSpan.FromMinutes(10));

        var query = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["response_type"] = "code",
            ["scope"] = "openid all",
            ["redirect_uri"] = _redirectUri,
            ["state"] = state,
            ["code_challenge_method"] = "S256",
            ["code_challenge"] = challenge
        };
        var parameters = string.Join("&", query.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        return new FrappeAuthorization($"{_baseUrl}/api/method/frappe.integrations.oauth2.authorize?{parameters}");
    }

    public async Task<FrappeAuthorizationResult> CompleteAsync(
        string code, string state, CancellationToken cancellationToken)
    {
        var key = $"frappe-oauth:{state}";
        if (!cache.TryGetValue<PendingAuthorization>(key, out var pending) || pending is null)
            throw new UnauthorizedAccessException("The Frappe authorization has expired or is invalid.");
        cache.Remove(key);

        using var tokenResponse = await httpClient.PostAsync(
            $"{_baseUrl}/api/method/frappe.integrations.oauth2.get_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["redirect_uri"] = _redirectUri,
                ["code_verifier"] = pending.CodeVerifier
            }), cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Frappe rejected the authorization code.");

        await using var tokenBody = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var tokenJson = await JsonDocument.ParseAsync(tokenBody, cancellationToken: cancellationToken);
        if (!tokenJson.RootElement.TryGetProperty("access_token", out var tokenElement) ||
            string.IsNullOrWhiteSpace(tokenElement.GetString()))
            throw new UnauthorizedAccessException("Frappe did not return an access token.");
        var accessToken = tokenElement.GetString()!;

        using var profileRequest = new HttpRequestMessage(HttpMethod.Get,
            $"{_baseUrl}/api/method/frappe.integrations.oauth2.openid_profile");
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var profileResponse = await httpClient.SendAsync(profileRequest, cancellationToken);
        if (!profileResponse.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Frappe did not confirm the account identity.");

        await using var profileBody = await profileResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var profileJson = await JsonDocument.ParseAsync(profileBody, cancellationToken: cancellationToken);
        if (!profileJson.RootElement.TryGetProperty("email", out var emailElement) ||
            !string.Equals(emailElement.GetString(), pending.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("The Frappe account does not match the PaquitoBot user.");

        return new FrappeAuthorizationResult(pending.UserId, pending.Email, accessToken);
    }

    private static string Required(IConfiguration configuration, string key) =>
        configuration[key] is { Length: > 0 } value ? value :
            throw new InvalidOperationException($"{key} is not configured.");

    private static string RandomBase64Url(int length) => Base64Url(RandomNumberGenerator.GetBytes(length));

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes)
        .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

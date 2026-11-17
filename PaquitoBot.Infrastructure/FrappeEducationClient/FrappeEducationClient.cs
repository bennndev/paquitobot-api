using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PaquitoBot.Application;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.FrappeEducationClient;

public sealed class FrappeEducationClient : IFrappeEducationClient
{
    private const string StudentFields =
        "name,student_code,first_name,last_name,student_name,student_email,student_mobile_number,date_of_birth,gender,enabled,modified";

    private readonly HttpClient _httpClient;

    public FrappeEducationClient(HttpClient httpClient, IConfiguration configuration)
    {
        var baseUrl = configuration["Frappe:BaseUrl"];

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException(
                "Configure 'Frappe:BaseUrl' with an absolute URL before using the Frappe client.");
        }

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(baseUri.ToString().TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var apiKey = configuration["Frappe:ApiKey"];
        var apiSecret = configuration["Frappe:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("token", $"{apiKey}:{apiSecret}");
        }
    }

    public async Task<Student?> GetStudentByExternalIdAsync(
        string studentExternalId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studentExternalId);

        var endpoint = $"api/resource/Student/{Uri.EscapeDataString(studentExternalId)}";
        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<FrappeResponse<FrappeStudent>>(
            cancellationToken: cancellationToken);

        return payload?.Data?.ToDomain();
    }

    public async Task<IReadOnlyList<Student>> GetAllStudentsAsync(
        CancellationToken cancellationToken = default)
    {
        var fields = Uri.EscapeDataString($"[\"{StudentFields.Replace(",", "\",\"")}\"]");
        var filters = Uri.EscapeDataString("[[\"enabled\",\"=\",1]]");
        var endpoint = $"api/resource/Student?fields={fields}&filters={filters}&limit_page_length=0";

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<FrappeResponse<List<FrappeStudent>>>(
            cancellationToken: cancellationToken);

        return payload?.Data?.Select(student => student.ToDomain()).ToList() ?? [];
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new FrappeAuthenticationException((int)response.StatusCode, details);
        }

        throw new FrappeApiException(
            (int)response.StatusCode,
            response.ReasonPhrase,
            details);
    }

    private sealed record FrappeResponse<T>(
        [property: JsonPropertyName("data")] T? Data);

    private sealed record FrappeStudent
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("student_code")]
        public string? StudentCode { get; init; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }

        [JsonPropertyName("student_name")]
        public string? StudentName { get; init; }

        [JsonPropertyName("student_email")]
        public string? Email { get; init; }

        [JsonPropertyName("student_mobile_number")]
        public string? Phone { get; init; }

        [JsonPropertyName("date_of_birth")]
        public DateOnly? BirthDate { get; init; }

        [JsonPropertyName("gender")]
        public string? Gender { get; init; }

        [JsonPropertyName("enabled")]
        public int Enabled { get; init; }

        [JsonPropertyName("modified")]
        public DateTime? Modified { get; init; }

        public Student ToDomain()
        {
            return new Student
            {
                FrappeExternalId = Name,
                StudentCode = StudentCode,
                FirstNames = FirstName,
                LastNames = LastName,
                FullName = StudentName,
                Email = Email,
                Phone = Phone,
                BirthDate = BirthDate,
                Gender = Gender,
                Status = Enabled == 1 ? "Active" : "Inactive",
                FrappeModifiedAt = Modified
            };
        }
    }
}

public class FrappeApiException : Exception
{
    public int StatusCode { get; }

    public FrappeApiException(int statusCode, string? message, string? details = null)
        : base($"Frappe API returned HTTP {statusCode}: {message}"
            + (string.IsNullOrWhiteSpace(details) ? string.Empty : $". {details}"))
    {
        StatusCode = statusCode;
    }
}

public sealed class FrappeAuthenticationException : FrappeApiException
{
    public FrappeAuthenticationException(int statusCode, string? details)
        : base(statusCode, "Authentication failed", details)
    {
    }
}

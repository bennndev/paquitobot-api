namespace PaquitoBot.Infrastructure.Integrations.Frappe;

public sealed class FrappeOptions
{
    public const string SectionName = "Frappe";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 15;
}

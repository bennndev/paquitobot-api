namespace PaquitoBot.Infrastructure.Configuration;

public sealed class FrappeOptions
{
    public const string SectionName = "FrappeOAuth";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 15;
}

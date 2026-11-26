namespace PaquitoBot.Infrastructure.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "JWTVariable";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
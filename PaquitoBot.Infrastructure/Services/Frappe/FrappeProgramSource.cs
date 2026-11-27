using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

public sealed class FrappeProgramSource(IFrappeClient client) : IFrappeSource<FrappeProgramDto>
{
    private const string DocType = "Program";

    private static readonly string[] Fields =
        ["name", "program_name", "program_abbreviation", "short_description", "modified"];

    public async Task<IReadOnlyList<FrappeProgramDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var programs = await client.GetListAsync<FrappeProgram>(DocType, Fields, cancellationToken);

        return programs.Select(program => program.ToDto()).ToList();
    }

    private sealed record FrappeProgram
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("program_name")]
        public string? ProgramName { get; init; }

        [JsonPropertyName("program_abbreviation")]
        public string? Abbreviation { get; init; }

        [JsonPropertyName("short_description")]
        public string? ShortDescription { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }

        public FrappeProgramDto ToDto() => new(
            Name, Abbreviation, ProgramName ?? Name, ShortDescription, null, FrappeFormats.ParseTimestamp(Modified));
    }
}

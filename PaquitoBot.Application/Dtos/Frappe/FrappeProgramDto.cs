namespace PaquitoBot.Application.Dtos.Frappe;

public record FrappeProgramDto(
    string ExternalId,
    string? Code,
    string Name,
    string? Description,
    string? Status,
    DateTime? ModifiedAt);

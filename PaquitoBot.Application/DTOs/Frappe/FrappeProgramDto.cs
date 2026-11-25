namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeProgramDto(
    string ExternalId,
    string? Code,
    string Name,
    string? Description,
    string? Status,
    DateTime? ModifiedAt);

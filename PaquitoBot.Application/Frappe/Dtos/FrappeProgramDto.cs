namespace PaquitoBot.Application.Frappe.Dtos;

public record FrappeProgramDto(
    string ExternalId,
    string? Code,
    string Name,
    string? Description,
    string? Status,
    DateTime? ModifiedAt);

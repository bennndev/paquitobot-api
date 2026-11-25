namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeCourseDto(
    string ExternalId,
    string? Code,
    string Name,
    string? Description,
    int? Credits,
    string? Status,
    DateTime? ModifiedAt);

namespace PaquitoBot.Application.Dtos.Frappe;

public record FrappeCourseDto(
    string ExternalId,
    string? Code,
    string Name,
    string? Description,
    int? Credits,
    string? Status,
    DateTime? ModifiedAt);

namespace PaquitoBot.Application.Frappe.Dtos;

public record FrappeInstructorDto(
    string ExternalId,
    string? FirstName,
    string? LastName,
    string? FullName,
    string? Email,
    string? Phone,
    string? Status,
    DateTime? ModifiedAt);

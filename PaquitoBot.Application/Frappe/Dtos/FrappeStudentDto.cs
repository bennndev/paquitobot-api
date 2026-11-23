namespace PaquitoBot.Application.Frappe.Dtos;

public record FrappeStudentDto(
    string ExternalId,
    string? StudentCode,
    string? FirstNames,
    string? LastNames,
    string? FullName,
    string? Email,
    string? Phone,
    DateOnly? BirthDate,
    string? Gender,
    string? Status,
    DateTime? ModifiedAt);

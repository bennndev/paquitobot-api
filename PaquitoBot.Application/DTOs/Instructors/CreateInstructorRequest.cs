namespace PaquitoBot.Application.DTOs.Instructors;

public record CreateInstructorRequest(
    string? FrappeExternalId = null,
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    string? Status = null);

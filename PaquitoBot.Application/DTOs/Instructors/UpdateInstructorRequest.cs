namespace PaquitoBot.Application.DTOs.Instructors;

public record UpdateInstructorRequest(
    string? FirstName = null,
    string? LastName = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    string? Status = null);

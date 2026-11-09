namespace PaquitoBot.Application.DTOs.Students;

public record CreateStudentRequest(
    string FrappeExternalId,
    string? StudentCode = null,
    string? FirstNames = null,
    string? LastNames = null,
    string? FullName = null,
    string? Email = null,
    string? Phone = null,
    DateOnly? BirthDate = null,
    string? Gender = null,
    string? Status = null);

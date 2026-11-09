using PaquitoBot.Infrastructure.Models;

namespace PaquitoBot.Application.DTOs.Students;

public record StudentDto(
    Guid StudentId,
    string FrappeExternalId,
    string? StudentCode,
    string? FirstNames,
    string? LastNames,
    string? FullName,
    string? Email,
    string? Phone,
    DateOnly? BirthDate,
    string? Gender,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static StudentDto From(Student student)
    {
        return new StudentDto(
            student.StudentId,
            student.FrappeExternalId,
            student.StudentCode,
            student.FirstNames,
            student.LastNames,
            student.FullName,
            student.Email,
            student.Phone,
            student.BirthDate,
            student.Gender,
            student.Status,
            student.FrappeModifiedAt,
            student.SyncedAt,
            student.CreatedAt,
            student.UpdatedAt);
    }
}

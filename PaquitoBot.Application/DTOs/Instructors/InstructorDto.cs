using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.Instructors;

public record InstructorDto(
    Guid InstructorId,
    string? FrappeExternalId,
    string? FirstName,
    string? LastName,
    string? FullName,
    string? Email,
    string? Phone,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static InstructorDto From(Instructor instructor)
    {
        return new InstructorDto(
            instructor.InstructorId,
            instructor.FrappeExternalId,
            instructor.FirstName,
            instructor.LastName,
            instructor.FullName,
            instructor.Email,
            instructor.Phone,
            instructor.Status,
            instructor.FrappeModifiedAt,
            instructor.SyncedAt,
            instructor.CreatedAt,
            instructor.UpdatedAt);
    }
}

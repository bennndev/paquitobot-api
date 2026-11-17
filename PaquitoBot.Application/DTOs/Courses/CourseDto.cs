using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.Courses;

public record CourseDto(
    Guid CourseId,
    string FrappeExternalId,
    string? Code,
    string Name,
    string? Description,
    int? Credits,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static CourseDto From(Course course)
    {
        return new CourseDto(
            course.CourseId,
            course.FrappeExternalId,
            course.Code,
            course.Name,
            course.Description,
            course.Credits,
            course.Status,
            course.FrappeModifiedAt,
            course.SyncedAt,
            course.CreatedAt,
            course.UpdatedAt);
    }
}

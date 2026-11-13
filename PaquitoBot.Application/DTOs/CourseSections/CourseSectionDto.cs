using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.CourseSections;

public record CourseSectionDto(
    Guid SectionId,
    string? FrappeExternalId,
    Guid CourseId,
    Guid AcademicPeriodId,
    string? SectionCode,
    string? InstructorName,
    string? DeliveryMode,
    int? Capacity,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static CourseSectionDto From(CourseSection section)
    {
        return new CourseSectionDto(
            section.SectionId,
            section.FrappeExternalId,
            section.CourseId,
            section.AcademicPeriodId,
            section.SectionCode,
            section.InstructorName,
            section.DeliveryMode,
            section.Capacity,
            section.Status,
            section.FrappeModifiedAt,
            section.SyncedAt,
            section.CreatedAt,
            section.UpdatedAt);
    }
}

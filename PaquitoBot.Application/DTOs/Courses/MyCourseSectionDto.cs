namespace PaquitoBot.Application.DTOs.Courses;

public sealed record MyCourseSectionDto(Guid CourseId, Guid SectionId, string? SectionCode);

using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.DTOs.CourseSchedules;

public record CourseScheduleDto(
    Guid ScheduleId,
    string? FrappeExternalId,
    Guid SectionId,
    Guid? InstructorId,
    DateOnly ClassDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Location,
    string? DeliveryMode,
    string? Status,
    DateTime? FrappeModifiedAt,
    DateTime? SyncedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static CourseScheduleDto From(CourseSchedule schedule)
    {
        return new CourseScheduleDto(
            schedule.ScheduleId,
            schedule.FrappeExternalId,
            schedule.SectionId,
            schedule.InstructorId,
            schedule.ClassDate,
            schedule.StartTime,
            schedule.EndTime,
            schedule.Location,
            schedule.DeliveryMode,
            schedule.Status,
            schedule.FrappeModifiedAt,
            schedule.SyncedAt,
            schedule.CreatedAt,
            schedule.UpdatedAt);
    }
}

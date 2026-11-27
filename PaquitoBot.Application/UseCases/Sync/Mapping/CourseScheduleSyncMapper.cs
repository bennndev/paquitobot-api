using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class CourseScheduleSyncMapper
{
    public static void Apply(FrappeCourseScheduleDto source, CourseSchedule schedule,
        Guid sectionId, Guid? instructorId)
    {
        if (source.EndTime <= source.StartTime)
        {
            throw new InvalidOperationException("The end time must be after the start time.");
        }

        schedule.SectionId = sectionId;
        schedule.InstructorId = instructorId;
        schedule.ClassDate = source.ClassDate;
        schedule.StartTime = source.StartTime;
        schedule.EndTime = source.EndTime;
        schedule.Location = TextNormalizer.Optional(source.Location);

        if (source.DeliveryMode is not null)
        {
            schedule.DeliveryMode = TextNormalizer.Optional(source.DeliveryMode);
        }

        if (!string.IsNullOrWhiteSpace(source.Status))
        {
            schedule.Status = TextNormalizer.OptionalLower(source.Status);
        }
    }
}

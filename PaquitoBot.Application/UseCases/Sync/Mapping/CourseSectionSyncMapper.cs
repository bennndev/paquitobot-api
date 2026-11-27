using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class CourseSectionSyncMapper
{
    public static void Apply(FrappeCourseSectionDto source, CourseSection section, Guid courseId, Guid periodId)
    {
        section.CourseId = courseId;
        section.AcademicPeriodId = periodId;
        section.SectionCode = TextNormalizer.Optional(source.SectionCode);
        section.Capacity = source.Capacity;
        section.Status = TextNormalizer.OptionalLower(source.Status);

        if (source.InstructorName is not null)
        {
            section.InstructorName = TextNormalizer.Optional(source.InstructorName);
        }

        if (source.DeliveryMode is not null)
        {
            section.DeliveryMode = TextNormalizer.Optional(source.DeliveryMode);
        }
    }
}

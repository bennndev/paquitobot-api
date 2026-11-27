using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class SectionEnrollmentSyncMapper
{
    public static void Apply(FrappeSectionEnrollmentDto source, SectionEnrollment enrollment,
        Guid studentId, Guid sectionId)
    {
        enrollment.StudentId = studentId;
        enrollment.SectionId = sectionId;
        enrollment.EnrollmentDate = source.EnrollmentDate;
        enrollment.Status = TextNormalizer.OptionalLower(source.Status);
    }
}

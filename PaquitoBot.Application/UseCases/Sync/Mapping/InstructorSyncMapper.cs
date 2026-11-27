using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class InstructorSyncMapper
{
    public static void Apply(FrappeInstructorDto source, Instructor instructor)
    {
        instructor.FirstName = TextNormalizer.Optional(source.FirstName);
        instructor.LastName = TextNormalizer.Optional(source.LastName);
        instructor.FullName = TextNormalizer.Optional(source.FullName);
        instructor.Email = TextNormalizer.OptionalEmail(source.Email);
        instructor.Phone = TextNormalizer.Optional(source.Phone);

        if (!string.IsNullOrWhiteSpace(source.Status))
        {
            instructor.Status = TextNormalizer.OptionalLower(source.Status);
        }
    }
}

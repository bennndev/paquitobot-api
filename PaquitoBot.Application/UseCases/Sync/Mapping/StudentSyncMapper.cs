using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Mapping;

internal static class StudentSyncMapper
{
    public static void Apply(FrappeStudentDto source, Student student)
    {
        var firstNames = TextNormalizer.Optional(source.FirstNames);
        var lastNames = TextNormalizer.Optional(source.LastNames);

        student.StudentCode = TextNormalizer.Optional(source.StudentCode) ??
                              TextNormalizer.Required(source.ExternalId);
        student.FirstNames = firstNames;
        student.LastNames = lastNames;
        student.FullName = TextNormalizer.Optional(source.FullName) ?? BuildFullName(firstNames, lastNames);
        student.Email = TextNormalizer.OptionalEmail(source.Email);
        student.Phone = TextNormalizer.Optional(source.Phone);
        student.BirthDate = source.BirthDate;
        student.Gender = TextNormalizer.Optional(source.Gender);
        student.Status = TextNormalizer.OptionalLower(source.Status);
    }

    private static string? BuildFullName(string? firstNames, string? lastNames)
    {
        var fullName = $"{firstNames} {lastNames}".Trim();
        return fullName.Length == 0 ? null : fullName;
    }
}

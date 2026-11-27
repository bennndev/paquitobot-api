using System.Text.Json.Serialization;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Sync;

namespace PaquitoBot.Infrastructure.Services.Frappe;

/// <summary>
/// This Frappe Education version has no program enrollment doctype, so enrollments come from
/// "Student Applicant" records that already point to a student. An applicant carries no program:
/// it is resolved from its admission register, then from its course, then from the register's course.
/// </summary>
public sealed class FrappeProgramEnrollmentSource(IFrappeClient client, FrappeCourseCatalog catalog)
    : IFrappeSource<FrappeProgramEnrollmentDto>
{
    private static readonly string[] ApplicantFields =
        ["name", "student", "admission_register", "course", "academic_term", "application_date", "application_status", "modified"];

    private static readonly string[] RegisterFields = ["name", "program", "course"];

    public async Task<IReadOnlyList<FrappeProgramEnrollmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var applicants = await client.GetListAsync<FrappeApplicant>("Student Applicant", ApplicantFields, cancellationToken);
        var registers = await client.GetListAsync<FrappeRegister>("Admission Register", RegisterFields, cancellationToken);
        var courses = await catalog.GetAllAsync(cancellationToken);

        var registerById = registers.ToDictionary(register => register.Name);
        var programByCourse = courses
            .Where(course => !string.IsNullOrWhiteSpace(course.Program))
            .ToDictionary(course => course.Name, course => course.Program!);

        return applicants
            .Where(applicant => !string.IsNullOrWhiteSpace(applicant.Student))
            .Select(applicant => ToDto(applicant, registerById, programByCourse))
            .ToList();
    }

    private static FrappeProgramEnrollmentDto ToDto(
        FrappeApplicant applicant,
        IReadOnlyDictionary<string, FrappeRegister> registers,
        IReadOnlyDictionary<string, string> programByCourse)
    {
        FrappeRegister? register = null;

        if (!string.IsNullOrWhiteSpace(applicant.AdmissionRegister))
        {
            registers.TryGetValue(applicant.AdmissionRegister, out register);
        }

        var program = FirstNonEmpty(register?.Program)
            ?? ProgramOfCourse(applicant.Course, programByCourse)
            ?? ProgramOfCourse(register?.Course, programByCourse);

        return new FrappeProgramEnrollmentDto(
            applicant.Name,
            applicant.Student!,
            program,
            FirstNonEmpty(applicant.AcademicTerm),
            applicant.ApplicationDate,
            applicant.ApplicationStatus,
            FrappeFormats.ParseTimestamp(applicant.Modified));
    }

    private static string? ProgramOfCourse(string? course, IReadOnlyDictionary<string, string> programByCourse)
        => !string.IsNullOrWhiteSpace(course) && programByCourse.TryGetValue(course, out var program) ? program : null;

    private static string? FirstNonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record FrappeApplicant
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("student")]
        public string? Student { get; init; }

        [JsonPropertyName("admission_register")]
        public string? AdmissionRegister { get; init; }

        [JsonPropertyName("course")]
        public string? Course { get; init; }

        [JsonPropertyName("academic_term")]
        public string? AcademicTerm { get; init; }

        [JsonPropertyName("application_date")]
        public DateOnly? ApplicationDate { get; init; }

        [JsonPropertyName("application_status")]
        public string? ApplicationStatus { get; init; }

        [JsonPropertyName("modified")]
        public string? Modified { get; init; }
    }

    private sealed record FrappeRegister
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("program")]
        public string? Program { get; init; }

        [JsonPropertyName("course")]
        public string? Course { get; init; }
    }

}

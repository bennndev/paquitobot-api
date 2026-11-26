using PaquitoBot.Application.Dtos.Sync;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Services.Frappe.Sync;

public class SectionEnrollmentSynchronizer(
    IFrappeSource<FrappeSectionEnrollmentDto> source,
    IFrappeSyncedRepository<SectionEnrollment> repository,
    IFrappeSyncedRepository<Student> students,
    IFrappeSyncedRepository<CourseSection> sections,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeSectionEnrollmentDto, SectionEnrollment>(repository, unitOfWork, timeProvider)
{
    private IReadOnlyDictionary<string, Student> _knownStudents = new Dictionary<string, Student>();
    private IReadOnlyDictionary<string, CourseSection> _knownSections = new Dictionary<string, CourseSection>();

    public override string EntityName => nameof(SectionEnrollment);

    public override int Order => SyncOrder.SectionDetails;

    protected override Task<IReadOnlyList<FrappeSectionEnrollmentDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override async Task PrepareAsync(
        IReadOnlyList<FrappeSectionEnrollmentDto> items,
        CancellationToken cancellationToken)
    {
        _knownStudents = await students.GetByFrappeExternalIdsAsync(
            items.Select(i => i.StudentExternalId.Trim()), cancellationToken);
        _knownSections = await sections.GetByFrappeExternalIdsAsync(
            items.Select(i => i.SectionExternalId.Trim()), cancellationToken);
    }

    protected override string GetExternalId(FrappeSectionEnrollmentDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeSectionEnrollmentDto external) => external.ModifiedAt;

    protected override SectionEnrollment Create(FrappeSectionEnrollmentDto external)
    {
        var enrollment = new SectionEnrollment { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, enrollment);

        return enrollment;
    }

    protected override void Apply(FrappeSectionEnrollmentDto external, SectionEnrollment enrollment)
    {
        var studentId = external.StudentExternalId.Trim();
        var sectionId = external.SectionExternalId.Trim();

        if (!_knownStudents.TryGetValue(studentId, out var student))
        {
            throw new InvalidOperationException($"Student '{studentId}' was not found.");
        }

        if (!_knownSections.TryGetValue(sectionId, out var section))
        {
            throw new InvalidOperationException($"Section '{sectionId}' was not found.");
        }

        enrollment.StudentId = student.StudentId;
        enrollment.SectionId = section.SectionId;
        enrollment.EnrollmentDate = external.EnrollmentDate;
        enrollment.Status = TextNormalizer.OptionalLower(external.Status);
    }
}

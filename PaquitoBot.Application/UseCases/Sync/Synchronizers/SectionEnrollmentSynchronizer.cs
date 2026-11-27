using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

namespace PaquitoBot.Application.UseCases.Sync.Synchronizers;

/// <summary>
/// Maps Frappe course enrollments to the student-section link. Both the student and the section
/// are required parents resolved by external id.
/// </summary>
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

    protected override Task<SectionEnrollment?> FindByLocalIdentityAsync(FrappeSectionEnrollmentDto item,
        CancellationToken cancellationToken)
    {
        if (!_knownStudents.TryGetValue(item.StudentExternalId.Trim(), out var student) ||
            !_knownSections.TryGetValue(item.SectionExternalId.Trim(), out var section))
        {
            return Task.FromResult<SectionEnrollment?>(null);
        }

        return Repository.FirstOrDefaultAsync(enrollment =>
            enrollment.StudentId == student.StudentId && enrollment.SectionId == section.SectionId,
            cancellationToken);
    }

    protected override void AttachExternalId(SectionEnrollment enrollment, string externalId)
    {
        enrollment.FrappeExternalId = externalId;
    }

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

        SectionEnrollmentSyncMapper.Apply(external, enrollment, student.StudentId, section.SectionId);
    }
}

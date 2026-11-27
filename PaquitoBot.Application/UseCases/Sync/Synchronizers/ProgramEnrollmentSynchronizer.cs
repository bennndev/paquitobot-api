using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Sync.Synchronizers;

/// <summary>
/// Maps Frappe enrollments to program enrollments. Student and program are required parents,
/// the academic period is optional; all are resolved by external id.
/// </summary>
public class ProgramEnrollmentSynchronizer(
    IFrappeSource<FrappeProgramEnrollmentDto> source,
    IFrappeSyncedRepository<ProgramEnrollment> repository,
    IFrappeSyncedRepository<Student> students,
    IFrappeSyncedRepository<Program> programs,
    IFrappeSyncedRepository<AcademicPeriod> periods,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeProgramEnrollmentDto, ProgramEnrollment>(repository, unitOfWork, timeProvider)
{
    private IReadOnlyDictionary<string, Student> _knownStudents = new Dictionary<string, Student>();
    private IReadOnlyDictionary<string, Program> _knownPrograms = new Dictionary<string, Program>();
    private IReadOnlyDictionary<string, AcademicPeriod> _knownPeriods = new Dictionary<string, AcademicPeriod>();
    private readonly Dictionary<(Guid StudentId, Guid ProgramId, Guid PeriodId), Guid> _unlinked = [];

    public override string EntityName => nameof(ProgramEnrollment);

    public override int Order => SyncOrder.SectionDetails;

    protected override Task<IReadOnlyList<FrappeProgramEnrollmentDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override async Task PrepareAsync(
        IReadOnlyList<FrappeProgramEnrollmentDto> items,
        CancellationToken cancellationToken)
    {
        _knownStudents = await students.GetByFrappeExternalIdsAsync(
            items.Select(i => i.StudentExternalId.Trim()), cancellationToken);

        _knownPrograms = await programs.GetByFrappeExternalIdsAsync(
            items.Where(i => !string.IsNullOrWhiteSpace(i.ProgramExternalId)).Select(i => i.ProgramExternalId!.Trim()),
            cancellationToken);

        _knownPeriods = await periods.GetByFrappeExternalIdsAsync(
            items.Where(i => !string.IsNullOrWhiteSpace(i.AcademicPeriodExternalId)).Select(i => i.AcademicPeriodExternalId!.Trim()),
            cancellationToken);

        _unlinked.Clear();
        var uniqueRemoteKeys = items
            .Select(TryGetKey)
            .Where(key => key.HasValue)
            .Select(key => key!.Value)
            .GroupBy(key => key)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToHashSet();

        var localEnrollments = await Repository.GetAllAsync(cancellationToken);
        foreach (var group in localEnrollments
                     .Where(enrollment => enrollment.AcademicPeriodId.HasValue)
                     .GroupBy(enrollment => (enrollment.StudentId, enrollment.ProgramId,
                         PeriodId: enrollment.AcademicPeriodId!.Value)))
        {
            var records = group.ToList();
            if (records.Count == 1 && records[0].FrappeExternalId is null && uniqueRemoteKeys.Contains(group.Key))
            {
                _unlinked.Add(group.Key, records[0].ProgramEnrollmentId);
            }
        }
    }

    protected override async Task<ProgramEnrollment?> FindUnlinkedAsync(FrappeProgramEnrollmentDto item,
        CancellationToken cancellationToken)
    {
        var key = TryGetKey(item);
        if (!key.HasValue || !_unlinked.Remove(key.Value, out var enrollmentId))
        {
            return null;
        }

        var enrollment = await Repository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null || enrollment.FrappeExternalId is not null)
        {
            return null;
        }

        return enrollment;
    }

    protected override void AttachExternalId(ProgramEnrollment enrollment, string externalId)
    {
        enrollment.FrappeExternalId = externalId;
    }

    private (Guid StudentId, Guid ProgramId, Guid PeriodId)? TryGetKey(FrappeProgramEnrollmentDto item)
    {
        var studentId = TextNormalizer.Optional(item.StudentExternalId);
        var programId = TextNormalizer.Optional(item.ProgramExternalId);
        var periodId = TextNormalizer.Optional(item.AcademicPeriodExternalId);
        if (studentId is null || programId is null || periodId is null ||
            !_knownStudents.TryGetValue(studentId, out var student) ||
            !_knownPrograms.TryGetValue(programId, out var program) ||
            !_knownPeriods.TryGetValue(periodId, out var period))
        {
            return null;
        }

        return (student.StudentId, program.ProgramId, period.AcademicPeriodId);
    }

    protected override string GetExternalId(FrappeProgramEnrollmentDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeProgramEnrollmentDto external) => external.ModifiedAt;

    protected override ProgramEnrollment Create(FrappeProgramEnrollmentDto external)
    {
        var enrollment = new ProgramEnrollment { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, enrollment);

        return enrollment;
    }

    protected override void Apply(FrappeProgramEnrollmentDto external, ProgramEnrollment enrollment)
    {
        var studentId = external.StudentExternalId.Trim();

        if (!_knownStudents.TryGetValue(studentId, out var student))
        {
            throw new InvalidOperationException($"Student '{studentId}' was not found.");
        }

        var programId = TextNormalizer.Optional(external.ProgramExternalId)
            ?? throw new InvalidOperationException("The program could not be resolved for this enrollment.");

        if (!_knownPrograms.TryGetValue(programId, out var program))
        {
            throw new InvalidOperationException($"Program '{programId}' was not found.");
        }

        enrollment.StudentId = student.StudentId;
        enrollment.ProgramId = program.ProgramId;
        enrollment.AcademicPeriodId = ResolvePeriod(external.AcademicPeriodExternalId);
        enrollment.EnrollmentDate = external.EnrollmentDate;
        enrollment.Status = TextNormalizer.OptionalLower(external.Status);
    }

    private Guid? ResolvePeriod(string? periodExternalId)
    {
        var id = TextNormalizer.Optional(periodExternalId);

        if (id is null)
        {
            return null;
        }

        if (!_knownPeriods.TryGetValue(id, out var period))
        {
            throw new InvalidOperationException($"Academic period '{id}' was not found.");
        }

        return period.AcademicPeriodId;
    }
}

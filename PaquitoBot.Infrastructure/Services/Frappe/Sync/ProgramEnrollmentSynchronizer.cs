using PaquitoBot.Application.Dtos.Sync;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Services.Frappe.Sync;

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

using PaquitoBot.Application.Dtos.Sync;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Helpers;
using PaquitoBot.Application.Dtos.Frappe;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Services.Frappe.Sync;

public class StudentSynchronizer(
    IFrappeSource<FrappeStudentDto> source,
    IFrappeSyncedRepository<Student> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeStudentDto, Student>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => nameof(Student);

    public override int Order => SyncOrder.MasterData;

    protected override Task<IReadOnlyList<FrappeStudentDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override string GetExternalId(FrappeStudentDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeStudentDto external) => external.ModifiedAt;

    protected override Student Create(FrappeStudentDto external)
    {
        var student = new Student { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, student);

        return student;
    }

    protected override void Apply(FrappeStudentDto external, Student student)
    {
        var firstNames = TextNormalizer.Optional(external.FirstNames);
        var lastNames = TextNormalizer.Optional(external.LastNames);

        student.StudentCode = TextNormalizer.Optional(external.StudentCode);
        student.FirstNames = firstNames;
        student.LastNames = lastNames;
        student.FullName = TextNormalizer.Optional(external.FullName) ?? BuildFullName(firstNames, lastNames);
        student.Email = TextNormalizer.OptionalEmail(external.Email);
        student.Phone = TextNormalizer.Optional(external.Phone);
        student.BirthDate = external.BirthDate;
        student.Gender = TextNormalizer.Optional(external.Gender);
        student.Status = TextNormalizer.OptionalLower(external.Status);
    }

    private static string? BuildFullName(string? firstNames, string? lastNames)
    {
        var fullName = $"{firstNames} {lastNames}".Trim();

        return fullName.Length == 0 ? null : fullName;
    }
}

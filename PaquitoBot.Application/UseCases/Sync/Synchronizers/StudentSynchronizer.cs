using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

namespace PaquitoBot.Application.UseCases.Sync.Synchronizers;

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
        StudentSyncMapper.Apply(external, student);
    }
}

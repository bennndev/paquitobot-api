using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Sync;

public class InstructorSynchronizer(
    IFrappeSource<FrappeInstructorDto> source,
    IFrappeSyncedRepository<Instructor> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeInstructorDto, Instructor>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => nameof(Instructor);

    public override int Order => SyncOrder.MasterData;

    protected override Task<IReadOnlyList<FrappeInstructorDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override string GetExternalId(FrappeInstructorDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeInstructorDto external) => external.ModifiedAt;

    protected override Instructor Create(FrappeInstructorDto external)
    {
        var entity = new Instructor { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, entity);

        return entity;
    }

    protected override void Apply(FrappeInstructorDto external, Instructor entity)
    {
        entity.FirstName = TextNormalizer.Optional(external.FirstName);
        entity.LastName = TextNormalizer.Optional(external.LastName);
        entity.FullName = TextNormalizer.Optional(external.FullName);
        entity.Email = TextNormalizer.OptionalEmail(external.Email);
        entity.Phone = TextNormalizer.Optional(external.Phone);
        entity.Status = TextNormalizer.OptionalLower(external.Status);
    }
}

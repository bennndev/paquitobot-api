using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Common.Text;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

namespace PaquitoBot.Application.UseCases.Sync.Synchronizers;

public class ProgramSynchronizer(
    IFrappeSource<FrappeProgramDto> source,
    IFrappeSyncedRepository<Program> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : FrappeSynchronizerBase<FrappeProgramDto, Program>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => nameof(Program);

    public override int Order => SyncOrder.MasterData;

    protected override Task<IReadOnlyList<FrappeProgramDto>> FetchAsync(CancellationToken cancellationToken)
        => source.GetAllAsync(cancellationToken);

    protected override string GetExternalId(FrappeProgramDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeProgramDto external) => external.ModifiedAt;

    protected override Program Create(FrappeProgramDto external)
    {
        var entity = new Program { FrappeExternalId = TextNormalizer.Required(external.ExternalId) };
        Apply(external, entity);

        return entity;
    }

    protected override void Apply(FrappeProgramDto external, Program entity)
    {
        ProgramSyncMapper.Apply(external, entity);
    }
}

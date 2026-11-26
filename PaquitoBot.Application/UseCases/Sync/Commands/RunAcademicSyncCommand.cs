using PaquitoBot.Application.Dtos.Sync;
using PaquitoBot.Application.Services;
using MediatR;

namespace PaquitoBot.Application.UseCases.Sync.Commands;



public record RunAcademicSyncCommand : IRequest<AcademicSyncResponse>;

internal sealed class RunAcademicSyncCommandHandler(IAcademicSyncService service)
    : IRequestHandler<RunAcademicSyncCommand, AcademicSyncResponse>
{
    public Task<AcademicSyncResponse> Handle(RunAcademicSyncCommand request, CancellationToken cancellationToken)
        => service.ExecuteAsync(cancellationToken);
}

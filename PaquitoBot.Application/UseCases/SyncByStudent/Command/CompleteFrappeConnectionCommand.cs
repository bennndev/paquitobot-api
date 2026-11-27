using MediatR;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Application.Services.SyncByStudent;

namespace PaquitoBot.Application.UseCases.SyncByStudent.Command;

public record CompleteFrappeConnectionCommand(string Code, string State) : IRequest<FrappeConnectionResponse>;

internal sealed class CompleteFrappeConnectionCommandHandler(
    IUnitOfWork unitOfWork, IFrappeOAuthService oauth, IFrappeStudentSyncService sync,
    ITokenService tokenService, IFrappeSyncRunService syncRuns, FrappeStudentSnapshotPersister persister)
    : IRequestHandler<CompleteFrappeConnectionCommand, FrappeConnectionResponse>
{
    public async Task<FrappeConnectionResponse> Handle(CompleteFrappeConnectionCommand request, CancellationToken cancellationToken)
    {
        var result = await oauth.CompleteAsync(request.Code, request.State, cancellationToken);
        var user = await unitOfWork.Users.GetByIdAsync(result.UserId, cancellationToken);
        if (user is null || !user.IsActive ||
            !string.Equals(user.Email, result.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("The Frappe account does not match the PaquitoBot user.");

        var syncRunId = await syncRuns.StartAsync("student_snapshot", cancellationToken);
        int processed;
        try
        {
            var snapshot = await sync.FetchSnapshotAsync(result.AccessToken, cancellationToken);
            async Task<int> PersistWithinTransaction(CancellationToken token)
            {
                return await persister.PersistAsync(snapshot, user.UserId, result.Email, token);
            }

            processed = await unitOfWork.ExecuteInTransactionAsync(PersistWithinTransaction, cancellationToken);
        }
        catch (Exception exception)
        {
            unitOfWork.DiscardPendingChanges();
            await syncRuns.FailAsync(syncRunId, 0, exception, CancellationToken.None);
            throw;
        }

        await syncRuns.CompleteAsync(syncRunId, processed, cancellationToken);
        var studentId = await unitOfWork.UserStudents.GetPrimaryStudentIdAsync(user.UserId, cancellationToken);
        var token = tokenService.GenerateToken(user.UserId, user.Role, user.Email, studentId);
        return new FrappeConnectionResponse(user.UserId, result.Email,
            new FrappeSyncSummary(syncRunId, processed), token);
    }
}

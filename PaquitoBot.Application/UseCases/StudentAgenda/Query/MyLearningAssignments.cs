using MediatR;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.StudentAgenda.Query;

public sealed record LearningAssignment(
    string AssignmentId, string Title, string ContextId, string ContextName,
    DateOnly? DueDate, bool Submitted, DateOnly? SubmittedOn);

public sealed record MyLearningAssignments(string Status, IReadOnlyList<LearningAssignment> Assignments);

public sealed record GetMyLearningAssignmentsQuery : IRequest<MyLearningAssignments>;

public interface IMyLearningAssignmentsReader
{
    Task<MyLearningAssignments> ReadAsync(Guid userId, Guid studentId, CancellationToken cancellationToken);
}

internal sealed class GetMyLearningAssignmentsQueryHandler(
    IMyLearningAssignmentsReader reader, ICurrentUser currentUser)
    : IRequestHandler<GetMyLearningAssignmentsQuery, MyLearningAssignments>
{
    public Task<MyLearningAssignments> Handle(GetMyLearningAssignmentsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || currentUser.StudentId is not { } studentId)
            throw new UnauthorizedAccessException("The current account must be linked to a student.");

        return reader.ReadAsync(userId, studentId, cancellationToken);
    }
}

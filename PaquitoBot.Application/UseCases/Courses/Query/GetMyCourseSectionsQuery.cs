using MediatR;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Application.UseCases.Courses.Query;

public sealed record GetMyCourseSectionsQuery : IRequest<IReadOnlyList<MyCourseSectionDto>>;

internal sealed class GetMyCourseSectionsQueryHandler(IMyCourseSectionReader reader, ICurrentUser currentUser)
    : IRequestHandler<GetMyCourseSectionsQuery, IReadOnlyList<MyCourseSectionDto>>
{
    public Task<IReadOnlyList<MyCourseSectionDto>> Handle(
        GetMyCourseSectionsQuery request, CancellationToken cancellationToken)
    {
        var studentId = currentUser.StudentId
            ?? throw new UnauthorizedAccessException("The current token has no student_id. Sign in again after connecting Frappe.");

        return reader.GetByStudentIdAsync(studentId, cancellationToken);
    }
}

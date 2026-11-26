using MediatR;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.Courses.Query;

public record GetMyCoursesQuery : IRequest<IReadOnlyList<CourseDto>>;

internal sealed class GetMyCoursesQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetMyCoursesQuery, IReadOnlyList<CourseDto>>
{
    public async Task<IReadOnlyList<CourseDto>> Handle(GetMyCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = currentUser.StudentId
            ?? throw new UnauthorizedAccessException("The current token has no student_id. Sign in again after connecting Frappe.");
        var courses = await unitOfWork.Courses.GetByStudentIdAsync(studentId, cancellationToken);
        return courses.Select(CourseDto.From).ToList();
    }
}

using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Courses;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Queries;

public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto>;

internal sealed class GetCourseByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCourseByIdQuery, CourseDto>
{
    public async Task<CourseDto> Handle(
        GetCourseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var course = await unitOfWork.Repository<Course>()
            .GetByIdAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException("Course", request.CourseId);

        return CourseDto.From(course);
    }
}

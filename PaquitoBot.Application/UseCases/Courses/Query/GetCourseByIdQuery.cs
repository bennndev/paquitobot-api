using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Query;

public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto>;

public class GetCourseByIdQueryHandler(IUnitOfWork unitOfWork)
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

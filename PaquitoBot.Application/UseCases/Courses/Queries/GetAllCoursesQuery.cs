using MediatR;
using PaquitoBot.Application.Dtos.Courses;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Queries;

public record GetAllCoursesQuery : IRequest<IReadOnlyList<CourseDto>>;

internal sealed class GetAllCoursesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllCoursesQuery, IReadOnlyList<CourseDto>>
{
    public async Task<IReadOnlyList<CourseDto>> Handle(
        GetAllCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var courses = await unitOfWork.Repository<Course>()
            .GetAllAsync(cancellationToken);

        return courses.Select(CourseDto.From).ToList();
    }
}

using MediatR;
using PaquitoBot.Application.DTOs.Courses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Courses.Query;

public record GetAllCoursesQuery : IRequest<IReadOnlyList<CourseDto>>;

public class GetAllCoursesQueryHandler(IUnitOfWork unitOfWork)
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

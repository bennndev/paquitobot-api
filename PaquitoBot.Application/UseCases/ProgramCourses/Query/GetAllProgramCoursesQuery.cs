using MediatR;
using PaquitoBot.Application.DTOs.ProgramCourses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Query;

public record GetAllProgramCoursesQuery
    : IRequest<IReadOnlyList<ProgramCourseDto>>;

public class GetAllProgramCoursesQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProgramCoursesQuery, IReadOnlyList<ProgramCourseDto>>
{
    public async Task<IReadOnlyList<ProgramCourseDto>> Handle(
        GetAllProgramCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var links = await unitOfWork.Repository<ProgramCourse>()
            .GetAllAsync(cancellationToken);

        return links.Select(ProgramCourseDto.From).ToList();
    }
}

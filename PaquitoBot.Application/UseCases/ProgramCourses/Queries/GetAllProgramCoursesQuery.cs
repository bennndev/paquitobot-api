using MediatR;
using PaquitoBot.Application.Dtos.ProgramCourses;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Queries;

public record GetAllProgramCoursesQuery
    : IRequest<IReadOnlyList<ProgramCourseDto>>;

internal sealed class GetAllProgramCoursesQueryHandler(
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

using MediatR;
using PaquitoBot.Application.DTOs.ProgramCourses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Query;

public record GetAllProgramCoursesUseCaseQuery
    : IRequest<IReadOnlyList<ProgramCourseDto>>;

public class GetAllProgramCoursesUseCaseQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProgramCoursesUseCaseQuery, IReadOnlyList<ProgramCourseDto>>
{
    public async Task<IReadOnlyList<ProgramCourseDto>> Handle(
        GetAllProgramCoursesUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var links = await unitOfWork.Repository<ProgramCourse>()
            .GetAllAsync(cancellationToken);

        return links.Select(ProgramCourseDto.From).ToList();
    }
}

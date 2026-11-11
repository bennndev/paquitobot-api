using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.ProgramCourses;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Query;

public record GetProgramCourseByIdUseCaseQuery(Guid ProgramCourseId)
    : IRequest<ProgramCourseDto>;

public class GetProgramCourseByIdUseCaseQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetProgramCourseByIdUseCaseQuery, ProgramCourseDto>
{
    public async Task<ProgramCourseDto> Handle(
        GetProgramCourseByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var link = await unitOfWork.Repository<ProgramCourse>()
            .GetByIdAsync(request.ProgramCourseId, cancellationToken)
            ?? throw new NotFoundException("ProgramCourse", request.ProgramCourseId);

        return ProgramCourseDto.From(link);
    }
}

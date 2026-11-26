using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.ProgramCourses;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramCourses.Queries;

public record GetProgramCourseByIdQuery(Guid ProgramCourseId)
    : IRequest<ProgramCourseDto>;

internal sealed class GetProgramCourseByIdQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetProgramCourseByIdQuery, ProgramCourseDto>
{
    public async Task<ProgramCourseDto> Handle(
        GetProgramCourseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var link = await unitOfWork.Repository<ProgramCourse>()
            .GetByIdAsync(request.ProgramCourseId, cancellationToken)
            ?? throw new NotFoundException("ProgramCourse", request.ProgramCourseId);

        return ProgramCourseDto.From(link);
    }
}

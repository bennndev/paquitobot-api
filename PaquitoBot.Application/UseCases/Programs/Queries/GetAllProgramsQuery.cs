
using MediatR;
using PaquitoBot.Application.Dtos.Programs;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Queries;

public record GetAllProgramsQuery : IRequest<IReadOnlyList<ProgramDto>>;

internal sealed class GetAllProgramsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProgramsQuery, IReadOnlyList<ProgramDto>>
{
    public async Task<IReadOnlyList<ProgramDto>> Handle(
        GetAllProgramsQuery request,
        CancellationToken cancellationToken)
    {
        var programs = await unitOfWork.Repository<Program>()
            .GetAllAsync(cancellationToken);

        return programs.Select(ProgramDto.From).ToList();
    }
}

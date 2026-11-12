
using MediatR;
using PaquitoBot.Application.DTOs.Programs;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Query;

public record GetAllProgramsQuery : IRequest<IReadOnlyList<ProgramDto>>;

public class GetAllProgramsQueryHandler(IUnitOfWork unitOfWork)
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

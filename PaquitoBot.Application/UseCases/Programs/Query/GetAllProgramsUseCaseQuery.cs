
using MediatR;
using PaquitoBot.Application.DTOs.Programs;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Query;

public record GetAllProgramsUseCaseQuery : IRequest<IReadOnlyList<ProgramDto>>;

public class GetAllProgramsUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProgramsUseCaseQuery, IReadOnlyList<ProgramDto>>
{
    public async Task<IReadOnlyList<ProgramDto>> Handle(
        GetAllProgramsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var programs = await unitOfWork.Repository<Program>()
            .GetAllAsync(cancellationToken);

        return programs.Select(ProgramDto.From).ToList();
    }
}

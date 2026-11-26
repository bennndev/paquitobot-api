
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Programs;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Queries;

public record GetProgramByIdQuery(Guid ProgramId) : IRequest<ProgramDto>;

internal sealed class GetProgramByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProgramByIdQuery, ProgramDto>
{
    public async Task<ProgramDto> Handle(
        GetProgramByIdQuery request,
        CancellationToken cancellationToken)
    {
        var program = await unitOfWork.Repository<Program>()
            .GetByIdAsync(request.ProgramId, cancellationToken)
            ?? throw new NotFoundException("Program", request.ProgramId);

        return ProgramDto.From(program);
    }
}

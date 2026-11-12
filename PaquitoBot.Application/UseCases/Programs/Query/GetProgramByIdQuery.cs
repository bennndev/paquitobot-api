
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Programs;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Programs.Query;

public record GetProgramByIdQuery(Guid ProgramId) : IRequest<ProgramDto>;

public class GetProgramByIdQueryHandler(IUnitOfWork unitOfWork)
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

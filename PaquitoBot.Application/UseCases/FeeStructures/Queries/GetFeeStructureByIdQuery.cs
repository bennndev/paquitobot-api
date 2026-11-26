using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.FeeStructures;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Queries;

public record GetFeeStructureByIdQuery(Guid FeeStructureId) : IRequest<FeeStructureDto>;

internal sealed class GetFeeStructureByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetFeeStructureByIdQuery, FeeStructureDto>
{
    public async Task<FeeStructureDto> Handle(
        GetFeeStructureByIdQuery request,
        CancellationToken cancellationToken)
    {
        var feeStructure = await _unitOfWork.Repository<FeeStructure>()
                               .GetByIdAsync(request.FeeStructureId, cancellationToken)
                           ?? throw new NotFoundException("FeeStructure", request.FeeStructureId);

        return FeeStructureDto.From(feeStructure);
    }
}
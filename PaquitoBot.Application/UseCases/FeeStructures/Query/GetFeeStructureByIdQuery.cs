using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.FeeStructures;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Query;

public record GetFeeStructureByIdQuery(Guid FeeStructureId) : IRequest<FeeStructureDto>;

public class GetFeeStructureByIdQueryHandler(IUnitOfWork _unitOfWork)
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
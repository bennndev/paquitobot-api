using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.FeeStructures;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Query;

public record GetFeeStructureByIdUseCaseQuery(Guid FeeStructureId) : IRequest<FeeStructureDto>;

public class GetFeeStructureByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetFeeStructureByIdUseCaseQuery, FeeStructureDto>
{
    public async Task<FeeStructureDto> Handle(
        GetFeeStructureByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var feeStructure = await _unitOfWork.Repository<FeeStructure>()
                               .GetByIdAsync(request.FeeStructureId, cancellationToken)
                           ?? throw new NotFoundException("FeeStructure", request.FeeStructureId);

        return FeeStructureDto.From(feeStructure);
    }
}
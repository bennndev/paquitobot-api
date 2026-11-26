using MediatR;
using PaquitoBot.Application.Dtos.FeeStructures;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Queries;

public record GetAllFeeStructuresQuery : IRequest<IReadOnlyList<FeeStructureDto>>;

internal sealed class GetAllFeeStructuresQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllFeeStructuresQuery, IReadOnlyList<FeeStructureDto>>
{
    public async Task<IReadOnlyList<FeeStructureDto>> Handle(
        GetAllFeeStructuresQuery request,
        CancellationToken cancellationToken)
    {
        var feeStructures = await _unitOfWork.Repository<FeeStructure>().GetAllAsync(cancellationToken);

        return feeStructures.Select(FeeStructureDto.From).ToList();
    }
}
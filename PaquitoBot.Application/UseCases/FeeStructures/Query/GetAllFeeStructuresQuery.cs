using MediatR;
using PaquitoBot.Application.DTOs.FeeStructures;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Query;

public record GetAllFeeStructuresQuery : IRequest<IReadOnlyList<FeeStructureDto>>;

public class GetAllFeeStructuresQueryHandler(IUnitOfWork _unitOfWork)
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
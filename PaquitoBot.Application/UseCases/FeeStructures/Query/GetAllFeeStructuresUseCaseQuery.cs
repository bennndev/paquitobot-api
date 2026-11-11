using MediatR;
using PaquitoBot.Application.DTOs.FeeStructures;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeStructures.Query;

public record GetAllFeeStructuresUseCaseQuery : IRequest<IReadOnlyList<FeeStructureDto>>;

public class GetAllFeeStructuresUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllFeeStructuresUseCaseQuery, IReadOnlyList<FeeStructureDto>>
{
    public async Task<IReadOnlyList<FeeStructureDto>> Handle(
        GetAllFeeStructuresUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var feeStructures = await _unitOfWork.Repository<FeeStructure>().GetAllAsync(cancellationToken);

        return feeStructures.Select(FeeStructureDto.From).ToList();
    }
}
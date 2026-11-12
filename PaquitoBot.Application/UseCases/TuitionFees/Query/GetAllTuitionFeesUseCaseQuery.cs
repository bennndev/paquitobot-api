using MediatR;
using PaquitoBot.Application.DTOs.TuitionFees;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Query;

public record GetAllTuitionFeesUseCaseQuery : IRequest<IReadOnlyList<TuitionFeeDto>>;

public class GetAllTuitionFeesUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllTuitionFeesUseCaseQuery, IReadOnlyList<TuitionFeeDto>>
{
    public async Task<IReadOnlyList<TuitionFeeDto>> Handle(
        GetAllTuitionFeesUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var tuitionFees = await _unitOfWork.Repository<TuitionFee>().GetAllAsync(cancellationToken);

        return tuitionFees.Select(TuitionFeeDto.From).ToList();
    }
}
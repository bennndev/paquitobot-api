using MediatR;
using PaquitoBot.Application.DTOs.TuitionFees;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Query;

public record GetAllTuitionFeesQuery : IRequest<IReadOnlyList<TuitionFeeDto>>;

public class GetAllTuitionFeesQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllTuitionFeesQuery, IReadOnlyList<TuitionFeeDto>>
{
    public async Task<IReadOnlyList<TuitionFeeDto>> Handle(
        GetAllTuitionFeesQuery request,
        CancellationToken cancellationToken)
    {
        var tuitionFees = await _unitOfWork.Repository<TuitionFee>().GetAllAsync(cancellationToken);

        return tuitionFees.Select(TuitionFeeDto.From).ToList();
    }
}
using MediatR;
using PaquitoBot.Application.Dtos.TuitionFees;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Queries;

public record GetAllTuitionFeesQuery : IRequest<IReadOnlyList<TuitionFeeDto>>;

internal sealed class GetAllTuitionFeesQueryHandler(IUnitOfWork _unitOfWork)
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
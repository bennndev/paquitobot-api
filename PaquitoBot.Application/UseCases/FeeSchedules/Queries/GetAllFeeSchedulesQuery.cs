using MediatR;
using PaquitoBot.Application.Dtos.FeeSchedules;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Queries;

public record GetAllFeeSchedulesQuery : IRequest<IReadOnlyList<FeeScheduleDto>>;

internal sealed class GetAllFeeSchedulesQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllFeeSchedulesQuery, IReadOnlyList<FeeScheduleDto>>
{
    public async Task<IReadOnlyList<FeeScheduleDto>> Handle(
        GetAllFeeSchedulesQuery request,
        CancellationToken cancellationToken)
    {
        var feeSchedules = await _unitOfWork.Repository<FeeSchedule>().GetAllAsync(cancellationToken);

        return feeSchedules.Select(FeeScheduleDto.From).ToList();
    }
}
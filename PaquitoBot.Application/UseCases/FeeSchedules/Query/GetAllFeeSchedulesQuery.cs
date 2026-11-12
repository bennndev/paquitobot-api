using MediatR;
using PaquitoBot.Application.DTOs.FeeSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Query;

public record GetAllFeeSchedulesQuery : IRequest<IReadOnlyList<FeeScheduleDto>>;

public class GetAllFeeSchedulesQueryHandler(IUnitOfWork _unitOfWork)
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
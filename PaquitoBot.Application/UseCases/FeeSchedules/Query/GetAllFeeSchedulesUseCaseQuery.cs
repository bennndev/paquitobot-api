using MediatR;
using PaquitoBot.Application.DTOs.FeeSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Query;

public record GetAllFeeSchedulesUseCaseQuery : IRequest<IReadOnlyList<FeeScheduleDto>>;

public class GetAllFeeSchedulesUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllFeeSchedulesUseCaseQuery, IReadOnlyList<FeeScheduleDto>>
{
    public async Task<IReadOnlyList<FeeScheduleDto>> Handle(
        GetAllFeeSchedulesUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var feeSchedules = await _unitOfWork.Repository<FeeSchedule>().GetAllAsync(cancellationToken);

        return feeSchedules.Select(FeeScheduleDto.From).ToList();
    }
}
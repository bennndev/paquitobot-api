using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.FeeSchedules;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Query;

public record GetFeeScheduleByIdUseCaseQuery(Guid FeeScheduleId) : IRequest<FeeScheduleDto>;

public class GetFeeScheduleByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetFeeScheduleByIdUseCaseQuery, FeeScheduleDto>
{
    public async Task<FeeScheduleDto> Handle(
        GetFeeScheduleByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var feeSchedule = await _unitOfWork.Repository<FeeSchedule>()
                              .GetByIdAsync(request.FeeScheduleId, cancellationToken)
                          ?? throw new NotFoundException("FeeSchedule", request.FeeScheduleId);

        return FeeScheduleDto.From(feeSchedule);
    }
}
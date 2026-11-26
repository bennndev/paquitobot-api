using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.FeeSchedules;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Queries;

public record GetFeeScheduleByIdQuery(Guid FeeScheduleId) : IRequest<FeeScheduleDto>;

internal sealed class GetFeeScheduleByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetFeeScheduleByIdQuery, FeeScheduleDto>
{
    public async Task<FeeScheduleDto> Handle(
        GetFeeScheduleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var feeSchedule = await _unitOfWork.Repository<FeeSchedule>()
                              .GetByIdAsync(request.FeeScheduleId, cancellationToken)
                          ?? throw new NotFoundException("FeeSchedule", request.FeeScheduleId);

        return FeeScheduleDto.From(feeSchedule);
    }
}
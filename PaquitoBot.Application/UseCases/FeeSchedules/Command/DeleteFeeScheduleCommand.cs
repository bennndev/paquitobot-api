using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Command;

public record DeleteFeeScheduleCommand(Guid FeeScheduleId) : IRequest;

public class DeleteFeeScheduleCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteFeeScheduleCommand>
{
    public async Task Handle(DeleteFeeScheduleCommand request, CancellationToken cancellationToken)
    {
        var feeSchedule = await _unitOfWork.Repository<FeeSchedule>()
                              .GetByIdAsync(request.FeeScheduleId, cancellationToken)
                          ?? throw new NotFoundException("FeeSchedule", request.FeeScheduleId);

        _unitOfWork.Repository<FeeSchedule>().Remove(feeSchedule);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
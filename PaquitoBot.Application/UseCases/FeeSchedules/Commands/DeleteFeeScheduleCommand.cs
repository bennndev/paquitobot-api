using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Commands;

public record DeleteFeeScheduleCommand(Guid FeeScheduleId) : IRequest;

internal sealed class DeleteFeeScheduleCommandHandler(IUnitOfWork _unitOfWork)
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
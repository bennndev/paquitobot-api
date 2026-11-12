using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.FeeSchedules.Command;

public record DeleteFeeScheduleUseCaseCommand(Guid FeeScheduleId) : IRequest;

public class DeleteFeeScheduleUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteFeeScheduleUseCaseCommand>
{
    public async Task Handle(DeleteFeeScheduleUseCaseCommand request, CancellationToken cancellationToken)
    {
        var feeSchedule = await _unitOfWork.Repository<FeeSchedule>()
                              .GetByIdAsync(request.FeeScheduleId, cancellationToken)
                          ?? throw new NotFoundException("FeeSchedule", request.FeeScheduleId);

        _unitOfWork.Repository<FeeSchedule>().Remove(feeSchedule);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
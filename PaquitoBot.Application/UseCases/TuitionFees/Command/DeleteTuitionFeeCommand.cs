using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Command;

public record DeleteTuitionFeeCommand(Guid TuitionFeeId) : IRequest;

public class DeleteTuitionFeeCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeleteTuitionFeeCommand>
{
    public async Task Handle(DeleteTuitionFeeCommand request, CancellationToken cancellationToken)
    {
        var tuitionFee = await _unitOfWork.Repository<TuitionFee>()
                             .GetByIdAsync(request.TuitionFeeId, cancellationToken)
                         ?? throw new NotFoundException("TuitionFee", request.TuitionFeeId);

        _unitOfWork.Repository<TuitionFee>().Remove(tuitionFee);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
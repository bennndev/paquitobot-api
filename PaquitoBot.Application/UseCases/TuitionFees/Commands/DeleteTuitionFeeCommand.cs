using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Commands;

public record DeleteTuitionFeeCommand(Guid TuitionFeeId) : IRequest;

internal sealed class DeleteTuitionFeeCommandHandler(IUnitOfWork _unitOfWork)
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
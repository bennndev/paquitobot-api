using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Command;

public record DeletePaymentCommand(Guid PaymentId) : IRequest;

public class DeletePaymentCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeletePaymentCommand>
{
    public async Task Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<Payment>()
                          .GetByIdAsync(request.PaymentId, cancellationToken)
                      ?? throw new NotFoundException("Payment", request.PaymentId);

        _unitOfWork.Repository<Payment>().Remove(payment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
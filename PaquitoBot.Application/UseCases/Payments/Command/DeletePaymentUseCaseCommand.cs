using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Command;

public record DeletePaymentUseCaseCommand(Guid PaymentId) : IRequest;

public class DeletePaymentUseCaseCommandHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<DeletePaymentUseCaseCommand>
{
    public async Task Handle(DeletePaymentUseCaseCommand request, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<Payment>()
                          .GetByIdAsync(request.PaymentId, cancellationToken)
                      ?? throw new NotFoundException("Payment", request.PaymentId);

        _unitOfWork.Repository<Payment>().Remove(payment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
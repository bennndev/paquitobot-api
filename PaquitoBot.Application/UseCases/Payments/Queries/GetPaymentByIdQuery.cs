using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Payments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Queries;

public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto>;

internal sealed class GetPaymentByIdQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(
        GetPaymentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<Payment>()
                          .GetByIdAsync(request.PaymentId, cancellationToken)
                      ?? throw new NotFoundException("Payment", request.PaymentId);

        return PaymentDto.From(payment);
    }
}
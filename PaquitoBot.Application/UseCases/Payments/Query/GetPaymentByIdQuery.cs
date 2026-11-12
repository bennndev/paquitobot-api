using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Query;

public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto>;

public class GetPaymentByIdQueryHandler(IUnitOfWork _unitOfWork)
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
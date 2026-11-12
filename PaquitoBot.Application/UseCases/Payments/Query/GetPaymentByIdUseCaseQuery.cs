using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Query;

public record GetPaymentByIdUseCaseQuery(Guid PaymentId) : IRequest<PaymentDto>;

public class GetPaymentByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetPaymentByIdUseCaseQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(
        GetPaymentByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<Payment>()
                          .GetByIdAsync(request.PaymentId, cancellationToken)
                      ?? throw new NotFoundException("Payment", request.PaymentId);

        return PaymentDto.From(payment);
    }
}
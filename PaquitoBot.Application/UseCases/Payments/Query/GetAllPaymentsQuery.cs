using MediatR;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Query;

public record GetAllPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetAllPaymentsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetAllPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.Repository<Payment>().GetAllAsync(cancellationToken);

        return payments.Select(PaymentDto.From).ToList();
    }
}
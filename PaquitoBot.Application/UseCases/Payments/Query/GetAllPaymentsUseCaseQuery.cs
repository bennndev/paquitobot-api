using MediatR;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Payments.Query;

public record GetAllPaymentsUseCaseQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetAllPaymentsUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllPaymentsUseCaseQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetAllPaymentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.Repository<Payment>().GetAllAsync(cancellationToken);

        return payments.Select(PaymentDto.From).ToList();
    }
}
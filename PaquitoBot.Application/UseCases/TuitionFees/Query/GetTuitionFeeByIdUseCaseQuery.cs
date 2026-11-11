using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.TuitionFees;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.TuitionFees.Query;

public record GetTuitionFeeByIdUseCaseQuery(Guid TuitionFeeId) : IRequest<TuitionFeeDto>;

public class GetTuitionFeeByIdUseCaseQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetTuitionFeeByIdUseCaseQuery, TuitionFeeDto>
{
    public async Task<TuitionFeeDto> Handle(
        GetTuitionFeeByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var tuitionFee = await _unitOfWork.Repository<TuitionFee>()
                             .GetByIdAsync(request.TuitionFeeId, cancellationToken)
                         ?? throw new NotFoundException("TuitionFee", request.TuitionFeeId);

        return TuitionFeeDto.From(tuitionFee);
    }
}
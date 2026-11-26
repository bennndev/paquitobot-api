
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.AcademicPeriods;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Queries;

public record GetAcademicPeriodByIdQuery(
    Guid AcademicPeriodId) : IRequest<AcademicPeriodDto>;

internal sealed class GetAcademicPeriodByIdQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<
        GetAcademicPeriodByIdQuery,
        AcademicPeriodDto>
{
    public async Task<AcademicPeriodDto> Handle(
        GetAcademicPeriodByIdQuery request,
        CancellationToken cancellationToken)
    {
        var period = await unitOfWork.Repository<AcademicPeriod>()
            .GetByIdAsync(
                request.AcademicPeriodId,
                cancellationToken)
            ?? throw new NotFoundException(
                "AcademicPeriod",
                request.AcademicPeriodId);

        return AcademicPeriodDto.From(period);
    }
}

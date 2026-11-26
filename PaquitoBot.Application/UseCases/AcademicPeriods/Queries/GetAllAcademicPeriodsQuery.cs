
using MediatR;
using PaquitoBot.Application.Dtos.AcademicPeriods;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Queries;

public record GetAllAcademicPeriodsQuery
    : IRequest<IReadOnlyList<AcademicPeriodDto>>;

internal sealed class GetAllAcademicPeriodsQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<
        GetAllAcademicPeriodsQuery,
        IReadOnlyList<AcademicPeriodDto>>
{
    public async Task<IReadOnlyList<AcademicPeriodDto>> Handle(
        GetAllAcademicPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var periods = await unitOfWork.Repository<AcademicPeriod>()
            .GetAllAsync(cancellationToken);

        return periods.Select(AcademicPeriodDto.From).ToList();
    }
}

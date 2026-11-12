
using MediatR;
using PaquitoBot.Application.DTOs.AcademicPeriods;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Query;

public record GetAllAcademicPeriodsQuery
    : IRequest<IReadOnlyList<AcademicPeriodDto>>;

public class GetAllAcademicPeriodsQueryHandler(
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

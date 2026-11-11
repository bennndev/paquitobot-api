
using MediatR;
using PaquitoBot.Application.DTOs.AcademicPeriods;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Query;

public record GetAllAcademicPeriodsUseCaseQuery
    : IRequest<IReadOnlyList<AcademicPeriodDto>>;

public class GetAllAcademicPeriodsUseCaseQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<
        GetAllAcademicPeriodsUseCaseQuery,
        IReadOnlyList<AcademicPeriodDto>>
{
    public async Task<IReadOnlyList<AcademicPeriodDto>> Handle(
        GetAllAcademicPeriodsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var periods = await unitOfWork.Repository<AcademicPeriod>()
            .GetAllAsync(cancellationToken);

        return periods.Select(AcademicPeriodDto.From).ToList();
    }
}

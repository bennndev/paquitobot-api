
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.AcademicPeriods;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Query;

public record GetAcademicPeriodByIdUseCaseQuery(
    Guid AcademicPeriodId) : IRequest<AcademicPeriodDto>;

public class GetAcademicPeriodByIdUseCaseQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<
        GetAcademicPeriodByIdUseCaseQuery,
        AcademicPeriodDto>
{
    public async Task<AcademicPeriodDto> Handle(
        GetAcademicPeriodByIdUseCaseQuery request,
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

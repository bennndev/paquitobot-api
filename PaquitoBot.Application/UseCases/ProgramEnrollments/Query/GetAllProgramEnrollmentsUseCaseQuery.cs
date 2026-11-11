using MediatR;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Query;

public record GetAllProgramEnrollmentsUseCaseQuery
    : IRequest<IReadOnlyList<ProgramEnrollmentDto>>;

public class GetAllProgramEnrollmentsUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProgramEnrollmentsUseCaseQuery, IReadOnlyList<ProgramEnrollmentDto>>
{
    public async Task<IReadOnlyList<ProgramEnrollmentDto>> Handle(
        GetAllProgramEnrollmentsUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var enrollments = await unitOfWork.Repository<ProgramEnrollment>()
            .GetAllAsync(cancellationToken);

        return enrollments.Select(ProgramEnrollmentDto.From).ToList();
    }
}

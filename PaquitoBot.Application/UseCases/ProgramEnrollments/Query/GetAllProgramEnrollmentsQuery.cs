using MediatR;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Query;

public record GetAllProgramEnrollmentsQuery
    : IRequest<IReadOnlyList<ProgramEnrollmentDto>>;

public class GetAllProgramEnrollmentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProgramEnrollmentsQuery, IReadOnlyList<ProgramEnrollmentDto>>
{
    public async Task<IReadOnlyList<ProgramEnrollmentDto>> Handle(
        GetAllProgramEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        var enrollments = await unitOfWork.Repository<ProgramEnrollment>()
            .GetAllAsync(cancellationToken);

        return enrollments.Select(ProgramEnrollmentDto.From).ToList();
    }
}

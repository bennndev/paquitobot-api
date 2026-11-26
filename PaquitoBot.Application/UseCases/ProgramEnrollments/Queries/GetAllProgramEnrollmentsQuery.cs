using MediatR;
using PaquitoBot.Application.Dtos.ProgramEnrollments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Queries;

public record GetAllProgramEnrollmentsQuery
    : IRequest<IReadOnlyList<ProgramEnrollmentDto>>;

internal sealed class GetAllProgramEnrollmentsQueryHandler(IUnitOfWork unitOfWork)
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

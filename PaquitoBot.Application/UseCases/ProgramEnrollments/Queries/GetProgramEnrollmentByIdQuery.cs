using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.ProgramEnrollments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Queries;

public record GetProgramEnrollmentByIdQuery(Guid ProgramEnrollmentId)
    : IRequest<ProgramEnrollmentDto>;

internal sealed class GetProgramEnrollmentByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProgramEnrollmentByIdQuery, ProgramEnrollmentDto>
{
    public async Task<ProgramEnrollmentDto> Handle(
        GetProgramEnrollmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var enrollment = await unitOfWork.Repository<ProgramEnrollment>()
            .GetByIdAsync(request.ProgramEnrollmentId, cancellationToken)
            ?? throw new NotFoundException("ProgramEnrollment", request.ProgramEnrollmentId);

        return ProgramEnrollmentDto.From(enrollment);
    }
}

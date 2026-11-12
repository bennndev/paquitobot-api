using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Query;

public record GetProgramEnrollmentByIdQuery(Guid ProgramEnrollmentId)
    : IRequest<ProgramEnrollmentDto>;

public class GetProgramEnrollmentByIdQueryHandler(IUnitOfWork unitOfWork)
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

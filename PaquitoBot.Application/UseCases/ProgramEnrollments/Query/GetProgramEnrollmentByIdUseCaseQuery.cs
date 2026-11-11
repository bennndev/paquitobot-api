using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.ProgramEnrollments;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Query;

public record GetProgramEnrollmentByIdUseCaseQuery(Guid ProgramEnrollmentId)
    : IRequest<ProgramEnrollmentDto>;

public class GetProgramEnrollmentByIdUseCaseQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProgramEnrollmentByIdUseCaseQuery, ProgramEnrollmentDto>
{
    public async Task<ProgramEnrollmentDto> Handle(
        GetProgramEnrollmentByIdUseCaseQuery request,
        CancellationToken cancellationToken)
    {
        var enrollment = await unitOfWork.Repository<ProgramEnrollment>()
            .GetByIdAsync(request.ProgramEnrollmentId, cancellationToken)
            ?? throw new NotFoundException("ProgramEnrollment", request.ProgramEnrollmentId);

        return ProgramEnrollmentDto.From(enrollment);
    }
}

using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Command;

public record DeleteProgramEnrollmentCommand(Guid ProgramEnrollmentId) : IRequest;

public class DeleteProgramEnrollmentCommandValidator
    : AbstractValidator<DeleteProgramEnrollmentCommand>
{
    public DeleteProgramEnrollmentCommandValidator()
    {
        RuleFor(x => x.ProgramEnrollmentId).NotEmpty();
    }
}

public class DeleteProgramEnrollmentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProgramEnrollmentCommand>
{
    public async Task Handle(
        DeleteProgramEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await unitOfWork.Repository<ProgramEnrollment>()
            .GetByIdAsync(request.ProgramEnrollmentId, cancellationToken)
            ?? throw new NotFoundException("ProgramEnrollment", request.ProgramEnrollmentId);

        unitOfWork.Repository<ProgramEnrollment>().Remove(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

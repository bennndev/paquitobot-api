using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Commands;

public record DeleteProgramEnrollmentCommand(Guid ProgramEnrollmentId) : IRequest;

public class DeleteProgramEnrollmentCommandValidator
    : AbstractValidator<DeleteProgramEnrollmentCommand>
{
    public DeleteProgramEnrollmentCommandValidator()
    {
        RuleFor(x => x.ProgramEnrollmentId).NotEmpty();
    }
}

internal sealed class DeleteProgramEnrollmentCommandHandler(IUnitOfWork unitOfWork)
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

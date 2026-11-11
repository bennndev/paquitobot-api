using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.ProgramEnrollments.Command;

public record DeleteProgramEnrollmentUseCaseCommand(Guid ProgramEnrollmentId) : IRequest;

public class DeleteProgramEnrollmentUseCaseCommandValidator
    : AbstractValidator<DeleteProgramEnrollmentUseCaseCommand>
{
    public DeleteProgramEnrollmentUseCaseCommandValidator()
    {
        RuleFor(x => x.ProgramEnrollmentId).NotEmpty();
    }
}

public class DeleteProgramEnrollmentUseCaseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProgramEnrollmentUseCaseCommand>
{
    public async Task Handle(
        DeleteProgramEnrollmentUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var enrollment = await unitOfWork.Repository<ProgramEnrollment>()
            .GetByIdAsync(request.ProgramEnrollmentId, cancellationToken)
            ?? throw new NotFoundException("ProgramEnrollment", request.ProgramEnrollmentId);

        unitOfWork.Repository<ProgramEnrollment>().Remove(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

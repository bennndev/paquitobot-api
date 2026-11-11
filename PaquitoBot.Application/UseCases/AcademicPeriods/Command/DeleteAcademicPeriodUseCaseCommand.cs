
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Command;

public record DeleteAcademicPeriodUseCaseCommand(
    Guid AcademicPeriodId) : IRequest;

public class DeleteAcademicPeriodUseCaseCommandValidator
    : AbstractValidator<DeleteAcademicPeriodUseCaseCommand>
{
    public DeleteAcademicPeriodUseCaseCommandValidator()
    {
        RuleFor(x => x.AcademicPeriodId)
            .NotEmpty();
    }
}

public class DeleteAcademicPeriodUseCaseCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAcademicPeriodUseCaseCommand>
{
    public async Task Handle(
        DeleteAcademicPeriodUseCaseCommand request,
        CancellationToken cancellationToken)
    {
        var period = await unitOfWork.Repository<AcademicPeriod>()
            .GetByIdAsync(
                request.AcademicPeriodId,
                cancellationToken)
            ?? throw new NotFoundException(
                "AcademicPeriod",
                request.AcademicPeriodId);

        unitOfWork.Repository<AcademicPeriod>()
            .Remove(period);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

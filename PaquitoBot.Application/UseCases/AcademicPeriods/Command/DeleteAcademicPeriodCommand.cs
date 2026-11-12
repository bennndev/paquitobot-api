
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.AcademicPeriods.Command;

public record DeleteAcademicPeriodCommand(
    Guid AcademicPeriodId) : IRequest;

public class DeleteAcademicPeriodCommandValidator
    : AbstractValidator<DeleteAcademicPeriodCommand>
{
    public DeleteAcademicPeriodCommandValidator()
    {
        RuleFor(x => x.AcademicPeriodId)
            .NotEmpty();
    }
}

public class DeleteAcademicPeriodCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAcademicPeriodCommand>
{
    public async Task Handle(
        DeleteAcademicPeriodCommand request,
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

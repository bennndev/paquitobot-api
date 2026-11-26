using PaquitoBot.Application.Services;
using FluentValidation;
using MediatR;
using PaquitoBot.Application.Dtos.StudentFinances;

namespace PaquitoBot.Application.UseCases.StudentFinances.Commands;

public record RunStudentFinancialReconciliationCommand(Guid StudentId)
    : IRequest<StudentFinancialReconciliationDto>;

public sealed class RunStudentFinancialReconciliationCommandValidator
    : AbstractValidator<RunStudentFinancialReconciliationCommand>
{
    public RunStudentFinancialReconciliationCommandValidator()
    {
        RuleFor(command => command.StudentId).NotEmpty();
    }
}

internal sealed class RunStudentFinancialReconciliationCommandHandler(IStudentFinancialReconciliationService service)
    : IRequestHandler<RunStudentFinancialReconciliationCommand, StudentFinancialReconciliationDto>
{
    public Task<StudentFinancialReconciliationDto> Handle(RunStudentFinancialReconciliationCommand request, CancellationToken cancellationToken)
        => service.ExecuteAsync(request.StudentId, cancellationToken);
}

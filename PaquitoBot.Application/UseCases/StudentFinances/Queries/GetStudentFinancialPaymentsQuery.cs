using FluentValidation;
using MediatR;
using PaquitoBot.Domain.Exceptions;
using PaquitoBot.Application.Dtos.Payments;
using PaquitoBot.Domain.Repositories;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.UseCases.StudentFinances.Queries;

public record GetStudentFinancialPaymentsQuery(
    Guid StudentId,
    string? Status = null,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<IReadOnlyList<PaymentDto>>;

public sealed class GetStudentFinancialPaymentsQueryValidator
    : AbstractValidator<GetStudentFinancialPaymentsQuery>
{
    public GetStudentFinancialPaymentsQueryValidator()
    {
        RuleFor(query => query.StudentId).NotEmpty();
        RuleFor(query => query.Status).MaximumLength(40);
        RuleFor(query => query.To)
            .Must((query, to) => !query.From.HasValue || !to.HasValue ||
                to.Value >= query.From.Value)
            .WithMessage("To must be on or after From.");
    }
}

internal sealed class GetStudentFinancialPaymentsQueryHandler(IFinancialReadRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetStudentFinancialPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetStudentFinancialPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException("The PaquitoBot user is not authenticated.");
        if ((!currentUser.IsAdministrator && !await repository.CanUserAccessStudentAsync(
                userId, request.StudentId, cancellationToken)) ||
            !await repository.StudentExistsAsync(request.StudentId, cancellationToken))
        {
            throw new NotFoundException("Student", request.StudentId);
        }

        var payments = await repository.GetFilteredPaymentsAsync(
            request.StudentId,
            string.IsNullOrWhiteSpace(request.Status) ? null : request.Status,
            request.From, request.To, cancellationToken);

        return payments.Select(PaymentDto.From).ToList();
    }
}

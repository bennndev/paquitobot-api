using FluentValidation;
using MediatR;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.DTOs.Payments;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.StudentFinances.Query;

public record GetStudentFinancialPaymentsQuery(
    Guid StudentId,
    Guid RequestingUserId,
    string? Status = null,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<IReadOnlyList<PaymentDto>>;

public sealed class GetStudentFinancialPaymentsQueryValidator
    : AbstractValidator<GetStudentFinancialPaymentsQuery>
{
    public GetStudentFinancialPaymentsQueryValidator()
    {
        RuleFor(query => query.StudentId).NotEmpty();
        RuleFor(query => query.RequestingUserId).NotEmpty();
        RuleFor(query => query.Status).MaximumLength(40);
        RuleFor(query => query.To)
            .Must((query, to) => !query.From.HasValue || !to.HasValue ||
                to.Value >= query.From.Value)
            .WithMessage("To must be on or after From.");
    }
}

public sealed class GetStudentFinancialPaymentsQueryHandler(IFinancialReadRepository repository)
    : IRequestHandler<GetStudentFinancialPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetStudentFinancialPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await repository.CanUserAccessStudentAsync(
                request.RequestingUserId, request.StudentId, cancellationToken) ||
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

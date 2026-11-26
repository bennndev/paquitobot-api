using FluentValidation;
using MediatR;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.Reports.Query;

public sealed record GetEnrollmentReportQuery(Guid? ProgramId, Guid? AcademicPeriodId)
    : IRequest<IReadOnlyList<EnrollmentReportRowDto>>;

public sealed class GetEnrollmentReportQueryValidator : AbstractValidator<GetEnrollmentReportQuery>
{
    public GetEnrollmentReportQueryValidator()
    {
        RuleFor(x => x.ProgramId).NotEqual(Guid.Empty).When(x => x.ProgramId.HasValue);
        RuleFor(x => x.AcademicPeriodId).NotEqual(Guid.Empty).When(x => x.AcademicPeriodId.HasValue);
    }
}

public sealed class GetEnrollmentReportQueryHandler(IReportReadRepository repository)
    : IRequestHandler<GetEnrollmentReportQuery, IReadOnlyList<EnrollmentReportRowDto>>
{
    public Task<IReadOnlyList<EnrollmentReportRowDto>> Handle(
        GetEnrollmentReportQuery request, CancellationToken cancellationToken) =>
        repository.GetEnrollmentsAsync(request.ProgramId, request.AcademicPeriodId, cancellationToken);
}

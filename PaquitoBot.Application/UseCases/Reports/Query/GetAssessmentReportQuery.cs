using FluentValidation;
using MediatR;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.Reports.Query;

public sealed record GetAssessmentReportQuery(Guid? ProgramId, Guid? AcademicPeriodId)
    : IRequest<IReadOnlyList<AssessmentReportRowDto>>;

public sealed class GetAssessmentReportQueryValidator : AbstractValidator<GetAssessmentReportQuery>
{
    public GetAssessmentReportQueryValidator()
    {
        RuleFor(x => x.ProgramId).NotEqual(Guid.Empty).When(x => x.ProgramId.HasValue);
        RuleFor(x => x.AcademicPeriodId).NotEqual(Guid.Empty).When(x => x.AcademicPeriodId.HasValue);
    }
}

public sealed class GetAssessmentReportQueryHandler(IReportReadRepository repository)
    : IRequestHandler<GetAssessmentReportQuery, IReadOnlyList<AssessmentReportRowDto>>
{
    public Task<IReadOnlyList<AssessmentReportRowDto>> Handle(
        GetAssessmentReportQuery request, CancellationToken cancellationToken) =>
        repository.GetAssessmentsAsync(request.ProgramId, request.AcademicPeriodId, cancellationToken);
}

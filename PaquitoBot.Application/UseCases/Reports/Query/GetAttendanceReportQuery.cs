using FluentValidation;
using MediatR;
using PaquitoBot.Application.DTOs.Reports;
using PaquitoBot.Application.Repositories;

namespace PaquitoBot.Application.UseCases.Reports.Query;

public sealed record GetAttendanceReportQuery(
    Guid? ProgramId, Guid? AcademicPeriodId, DateOnly? From, DateOnly? To)
    : IRequest<IReadOnlyList<AttendanceReportRowDto>>;

public sealed class GetAttendanceReportQueryValidator : AbstractValidator<GetAttendanceReportQuery>
{
    public GetAttendanceReportQueryValidator()
    {
        RuleFor(x => x.ProgramId).NotEqual(Guid.Empty).When(x => x.ProgramId.HasValue);
        RuleFor(x => x.AcademicPeriodId).NotEqual(Guid.Empty).When(x => x.AcademicPeriodId.HasValue);
        RuleFor(x => x.To).Must((query, to) => !query.From.HasValue || !to.HasValue || to >= query.From)
            .WithMessage("To must be on or after From.");
    }
}

public sealed class GetAttendanceReportQueryHandler(IReportReadRepository repository)
    : IRequestHandler<GetAttendanceReportQuery, IReadOnlyList<AttendanceReportRowDto>>
{
    public Task<IReadOnlyList<AttendanceReportRowDto>> Handle(
        GetAttendanceReportQuery request, CancellationToken cancellationToken) =>
        repository.GetAttendanceAsync(
            request.ProgramId, request.AcademicPeriodId, request.From, request.To, cancellationToken);
}

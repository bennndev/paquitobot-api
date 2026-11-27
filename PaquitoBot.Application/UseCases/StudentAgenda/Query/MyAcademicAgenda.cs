using FluentValidation;
using MediatR;
using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Application.UseCases.StudentAgenda.Query;

public sealed record AgendaClass(
    Guid ScheduleId, Guid CourseId, Guid SectionId, string CourseName, string? SectionCode,
    DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string? Location, string? Status);

public sealed record AgendaEvaluation(
    Guid AssessmentPlanId, Guid CourseId, Guid SectionId, string CourseName, string? SectionCode,
    string Title, DateOnly Date, string? Status);

public sealed record MyAcademicAgenda(IReadOnlyList<AgendaClass> Classes, IReadOnlyList<AgendaEvaluation> Evaluations);

public sealed record GetMyAcademicAgendaQuery(DateOnly From, DateOnly To) : IRequest<MyAcademicAgenda>;

public interface IMyAcademicAgendaReader
{
    Task<MyAcademicAgenda> ReadAsync(Guid studentId, DateOnly from, DateOnly to, CancellationToken cancellationToken);
}

public sealed class GetMyAcademicAgendaQueryValidator : AbstractValidator<GetMyAcademicAgendaQuery>
{
    public GetMyAcademicAgendaQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThanOrEqualTo(x => x.From);
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber < 90)
            .WithMessage("The agenda range cannot exceed 90 days.");
    }
}

internal sealed class GetMyAcademicAgendaQueryHandler(IMyAcademicAgendaReader reader, ICurrentUser currentUser)
    : IRequestHandler<GetMyAcademicAgendaQuery, MyAcademicAgenda>
{
    public Task<MyAcademicAgenda> Handle(GetMyAcademicAgendaQuery request, CancellationToken cancellationToken)
    {
        var studentId = currentUser.StudentId
            ?? throw new UnauthorizedAccessException("The current token has no student_id. Sign in again after connecting Frappe.");

        return reader.ReadAsync(studentId, request.From, request.To, cancellationToken);
    }
}

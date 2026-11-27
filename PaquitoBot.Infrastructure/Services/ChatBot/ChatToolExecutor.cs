using System.Text.Json;
using MediatR;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services.Authenticate;
using PaquitoBot.Application.Services.ChatBot;
using PaquitoBot.Application.UseCases.StudentFinances.Query;

namespace PaquitoBot.Infrastructure.Services.ChatBot;

public sealed class ChatToolExecutor(IUnitOfWork unitOfWork, ICurrentUser currentUser, ISender sender)
    : IChatToolExecutor
{
    // Ejecuta solo herramientas conocidas y usa el estudiante vinculado al usuario autenticado.
    public async Task<string> ExecuteAsync(string toolName, CancellationToken cancellationToken = default)
    {
        if (!ChatToolNames.Contains(toolName))
        {
            throw new InvalidOperationException("Gemini requested an unsupported tool.");
        }

        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("The authenticated user could not be identified.");
        if (currentUser.StudentId is not Guid studentId ||
            !await unitOfWork.UserStudents.PairExistsAsync(userId, studentId, cancellationToken))
        {
            return JsonSerializer.Serialize(new { error = "student_account_not_connected" });
        }

        if (toolName == ChatToolNames.Courses)
        {
            var courses = await unitOfWork.Courses.GetByStudentIdAsync(studentId, cancellationToken);
            return JsonSerializer.Serialize(courses.Select(course => new
            {
                course.Code,
                course.Name,
                course.Status
            }));
        }

        if (toolName == ChatToolNames.Assessments)
        {
            var assessments = await unitOfWork.Assessments.GetByStudentIdAsync(studentId, cancellationToken);
            return JsonSerializer.Serialize(assessments.Select(assessment => new
            {
                assessment.Title,
                assessment.AssessmentType,
                assessment.AssessmentDate,
                assessment.MaximumScore,
                assessment.Status
            }));
        }

        if (toolName == ChatToolNames.Grades)
        {
            var results = await unitOfWork.AssessmentResults.GetByStudentIdAsync(studentId, cancellationToken);
            // Si hay varias filas para una evaluación, entrega solo la más reciente.
            var latestResults = results.GroupBy(result => result.AssessmentId)
                .Select(group => group.OrderByDescending(result => result.UpdatedAt)
                    .ThenByDescending(result => result.CreatedAt)
                    .ThenByDescending(result => result.ResultId)
                    .First());
            return JsonSerializer.Serialize(latestResults.Select(result => new
            {
                result.Assessment.Title,
                result.Assessment.AssessmentDate,
                result.Grade,
                result.Assessment.MaximumScore,
                result.Notes
            }));
        }

        if (toolName == ChatToolNames.Attendance)
        {
            var records = await unitOfWork.AttendanceRecords.GetByStudentIdAsync(studentId, cancellationToken);
            return JsonSerializer.Serialize(records.Select(record => new
            {
                Course = record.Section.Course.Name,
                record.Section.SectionCode,
                record.AttendanceDate,
                record.Status,
                record.Notes
            }));
        }

        if (toolName == ChatToolNames.Finances)
        {
            // Reutiliza los saldos calculados por el caso de uso financiero.
            var statement = await sender.Send(
                new GetStudentFinancialStatementQuery(studentId, userId), cancellationToken);
            return JsonSerializer.Serialize(new
            {
                statement.AsOf,
                statement.Currencies,
                Obligations = statement.Obligations.Select(obligation => new
                {
                    obligation.Description,
                    obligation.CurrencyCode,
                    obligation.OriginalAmount,
                    obligation.ConfirmedPaidAmount,
                    obligation.OutstandingAmount,
                    obligation.DueDate,
                    obligation.CalculatedStatus,
                    obligation.ReconciliationRequired
                }),
                statement.PaymentsNotCounted
            });
        }

        if (toolName == ChatToolNames.Schedule)
        {
            var schedules = await unitOfWork.CourseSchedules.GetByStudentIdAsync(studentId, cancellationToken);
            return JsonSerializer.Serialize(schedules.Select(schedule => new
            {
                Course = schedule.Section.Course.Name,
                schedule.Section.SectionCode,
                schedule.ClassDate,
                schedule.StartTime,
                schedule.EndTime,
                schedule.Location,
                schedule.DeliveryMode,
                schedule.Status
            }));
        }

        if (toolName == ChatToolNames.AssessmentPlans)
        {
            var plans = await unitOfWork.AssessmentPlans.GetByStudentIdAsync(studentId, cancellationToken);
            return JsonSerializer.Serialize(plans.Select(plan => new
            {
                Course = plan.Section.Course.Name,
                plan.Section.SectionCode,
                plan.Title,
                plan.Description,
                plan.ScheduledDate,
                plan.MaximumScore,
                plan.Status
            }));
        }

        throw new InvalidOperationException("The chat tool has no executor.");
    }
}

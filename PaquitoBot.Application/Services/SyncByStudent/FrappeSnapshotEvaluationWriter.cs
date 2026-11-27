using System.Globalization;
using System.Text.Json;
using PaquitoBot.Application.Common.Frappe;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Domain.Models;
using PaquitoBot.Application.UseCases.Sync.Mapping;

using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotJson;
using static PaquitoBot.Application.Services.SyncByStudent.FrappeSnapshotWrites;

namespace PaquitoBot.Application.Services.SyncByStudent;

internal sealed class FrappeSnapshotEvaluationWriter(IUnitOfWork unitOfWork)
{
    public async Task<int> WriteAsync(JsonElement snapshot, Student student, string studentName,
        IReadOnlyDictionary<string, CourseSection> sections, DateTime now, CancellationToken cancellationToken)
    {
        var processed = 0;
        var assessments = new Dictionary<string, Assessment>();
        var assessmentPlans = new Dictionary<string, AssessmentPlan>();
        foreach (var row in Rows(snapshot, "plans"))
        {
            EnsureStudent(row, studentName);
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            var title = Optional(row, "assessment_name") ?? Optional(row, "subject") ?? name;
            var scheduledDate = Date(row, "schedule_date");
            var max = Number(row, "maximum_assessment_score");
            assessmentPlans[name] = await Upsert(unitOfWork.Repository<AssessmentPlan>(), name, entity =>
            {
                entity.SectionId = section.SectionId;
                entity.Title = title;
                entity.Description = Optional(row, "subject");
                entity.ScheduledDate = scheduledDate;
                entity.MaximumScore = max;
                entity.Status = "scheduled";
                entity.SyncedAt = now;
            }, cancellationToken);
            assessments[name] = await Upsert(unitOfWork.Repository<Assessment>(), name, entity =>
            {
                entity.SectionId = section.SectionId;
                entity.Title = title;
                entity.AssessmentType = Optional(row, "assignment_type");
                entity.AssessmentDate = scheduledDate;
                entity.MaximumScore = max;
                entity.Status = scheduledDate < DateOnly.FromDateTime(now) ? "completed" : "scheduled";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "results"))
        {
            EnsureStudent(row, studentName);
            var planName = Required(row, "assessment_plan");
            if (!assessments.TryGetValue(planName, out var assessment))
                throw new InvalidOperationException($"Assessment plan {planName} is missing from the snapshot.");
            assessment.Status = "graded";
            assessmentPlans[planName].Status = "completed";
            await Upsert(unitOfWork.Repository<AssessmentResult>(), Required(row, "name"), entity =>
            {
                entity.StudentId = student.StudentId;
                entity.AssessmentId = assessment.AssessmentId;
                entity.Grade = Number(row, "score");
                entity.Notes = Optional(row, "comment");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        foreach (var row in Rows(snapshot, "attendance"))
        {
            EnsureStudent(row, studentName);
            var section = sections[Required(row, "student_batch")];
            await Upsert(unitOfWork.Repository<AttendanceRecord>(), Required(row, "name"), entity =>
            {
                entity.StudentId = student.StudentId;
                entity.SectionId = section.SectionId;
                entity.AttendanceDate = Date(row, "date") ?? throw new InvalidOperationException("Attendance date is missing.");
                entity.Status = Required(row, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        return processed;
    }
}

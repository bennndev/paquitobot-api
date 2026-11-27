using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Application.UseCases.StudentAgenda.Query;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Services.Frappe;

internal sealed class FrappeLearningAssignmentsReader(
    PaquitoBotDbContext context, IFrappeClient frappe, ILogger<FrappeLearningAssignmentsReader> logger)
    : IMyLearningAssignmentsReader
{
    private const string AssignmentType = "LMS Assignment";

    public async Task<MyLearningAssignments> ReadAsync(Guid userId, Guid studentId, CancellationToken cancellationToken)
    {
        var externalId = await context.UserStudents.AsNoTracking()
            .Where(link => link.UserId == userId && link.StudentId == studentId && link.User.IsActive)
            .Select(link => link.Student.FrappeExternalId)
            .FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(externalId))
            return new("notLinked", []);

        try
        {
            var students = await ListAsync<EducationStudent>("Student", ["user"],
                [new("name", "=", externalId)], cancellationToken);
            // Education links Student.user to the same Frappe User used by Learning membership.
            var member = students.SingleOrDefault()?.User;
            if (string.IsNullOrWhiteSpace(member) || member is "Guest" or "Administrator")
                return new("notLinked", []);

            var placements = await GetPlacementsAsync(member, cancellationToken);
            var ids = placements.Select(row => row.AssignmentId).Distinct().ToArray();
            var assignments = await ByIdsAsync<Assignment>(AssignmentType, ["name", "title", "enable_scheduling", "schedule_end"],
                "name", ids, [], cancellationToken);
            var submissions = await ByIdsAsync<Submission>("LMS Assignment Submission", ["assignment", "creation"],
                "assignment", ids, [new("member", "=", member), new("docstatus", "<", 2)], cancellationToken);
            var assignmentsById = assignments.ToDictionary(row => row.Name, StringComparer.Ordinal);
            var submissionsByAssignment = submissions.ToLookup(row => row.Assignment, StringComparer.Ordinal);

            var rows = placements.Where(row => assignmentsById.ContainsKey(row.AssignmentId)).Distinct()
                .Select(row =>
                {
                    var assignment = assignmentsById[row.AssignmentId];
                    var submittedOn = submissionsByAssignment[row.AssignmentId].Max(item => ParseDate(item.Creation));
                    return new LearningAssignment(assignment.Name, assignment.Title, row.ContextId, row.ContextName,
                        assignment.EnableScheduling == 1 ? ParseDate(assignment.ScheduleEnd) : null,
                        submissionsByAssignment.Contains(row.AssignmentId), submittedOn);
                })
                .OrderBy(row => row.DueDate is null).ThenBy(row => row.DueDate).ThenBy(row => row.Title)
                .ToList();
            return new("ready", rows);
        }
        catch (FrappeException exception)
        {
            logger.LogWarning("Frappe Learning assignments could not be read ({Failure}).", exception.Failure);
            return new("unavailable", []);
        }
    }

    private async Task<List<Placement>> GetPlacementsAsync(string member, CancellationToken cancellationToken)
    {
        var courseMemberships = await ListAsync<CourseMembership>("LMS Enrollment", ["course"],
            [new("member", "=", member)], cancellationToken);
        var batchMemberships = await ListAsync<BatchMembership>("LMS Batch Enrollment", ["batch"],
            [new("member", "=", member)], cancellationToken);

        var courses = await ByIdsAsync<LearningContext>("LMS Course", ["name", "title"], "name",
            courseMemberships.Select(row => row.Course), [new("published", "=", 1)], cancellationToken);
        var batches = await ByIdsAsync<LearningContext>("LMS Batch", ["name", "title"], "name",
            batchMemberships.Select(row => row.Batch), [new("published", "=", 1)], cancellationToken);
        var lessons = await ByIdsAsync<Lesson>("Course Lesson", ["name", "course"], "course",
            courses.Select(row => row.Name), [], cancellationToken);

        var courseAssignments = await ByIdsAsync<Assessment>("LMS Lesson Assessment", ["parent", "assessment_name"], "parent",
            lessons.Select(row => row.Name),
            [new("parenttype", "=", "Course Lesson"), new("assessment_type", "=", AssignmentType), new("instructor_only", "=", 0)],
            cancellationToken, "Course Lesson");
        var batchAssignments = await ByIdsAsync<Assessment>("LMS Assessment", ["parent", "assessment_name"], "parent",
            batches.Select(row => row.Name),
            [new("parenttype", "=", "LMS Batch"), new("assessment_type", "=", AssignmentType)],
            cancellationToken, "LMS Batch");

        var inCourses = from assessment in courseAssignments
                        join lesson in lessons on assessment.Parent equals lesson.Name
                        join course in courses on lesson.Course equals course.Name
                        select new Placement(assessment.AssessmentName, $"lms:course:{course.Name}", course.Title);
        var inBatches = from assessment in batchAssignments
                        join batch in batches on assessment.Parent equals batch.Name
                        select new Placement(assessment.AssessmentName, $"lms:batch:{batch.Name}", batch.Title);
        return inCourses.Concat(inBatches).ToList();
    }

    private async Task<List<T>> ByIdsAsync<T>(string resource, string[] fields, string key,
        IEnumerable<string> ids, FrappeFilter[] filters, CancellationToken cancellationToken, string? parent = null)
        where T : notnull
    {
        var result = new List<T>();
        foreach (var group in ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().Chunk(20))
        {
            result.AddRange(await ListAsync<T>(resource, fields, [.. filters, new(key, "in", group)],
                cancellationToken, parent));
        }
        return result;
    }

    private async Task<List<T>> ListAsync<T>(string resource, string[] fields, FrappeFilter[] filters,
        CancellationToken cancellationToken, string? parent = null) where T : notnull
    {
        var rows = new List<T>();
        var query = new FrappeQuery(resource, fields, filters, ParentDocType: parent);
        while (true)
        {
            var page = await frappe.GetPageAsync<T>(query, cancellationToken);
            rows.AddRange(page.Items);
            if (page.NextOffset is not { } offset)
                return rows;
            if (offset <= query.Offset || offset > 5000)
                throw new FrappeException(FrappeFailure.InvalidResponse);
            query = query with { Offset = offset };
        }
    }

    private static DateOnly? ParseDate(string? value) =>
        value is { Length: >= 10 } &&
        DateOnly.TryParseExact(value.AsSpan(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    private sealed record EducationStudent(string? User);
    private sealed record CourseMembership(string Course);
    private sealed record BatchMembership(string Batch);
    private sealed record LearningContext(string Name, string Title);
    private sealed record Lesson(string Name, string Course);
    private sealed record Assessment(string Parent, string AssessmentName);
    private sealed record Assignment(string Name, string Title, int EnableScheduling, string? ScheduleEnd);
    private sealed record Submission(string Assignment, string Creation);
    private sealed record Placement(string AssignmentId, string ContextId, string ContextName);
}

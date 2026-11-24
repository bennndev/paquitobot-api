using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Services;

public sealed class FrappeStudentSyncService(
    HttpClient httpClient, IConfiguration configuration, PaquitoBotDbContext db) : IFrappeStudentSyncService
{
    public async Task<FrappeSyncSummary> SyncAsync(string accessToken, Guid userId, string email,
        CancellationToken cancellationToken)
    {
        var baseUrl = configuration["FrappeOAuth:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("FrappeOAuth:BaseUrl is not configured.");
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/api/method/paquitobot_student_snapshot");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Frappe student snapshot failed ({(int)response.StatusCode}).");
        await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
        var snapshot = document.RootElement.GetProperty("message").GetProperty("snapshot");
        var studentRow = snapshot.GetProperty("student");
        if (!string.Equals(Required(studentRow, "email_address"), email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Frappe returned a different student's records.");

        var startedAt = DateTime.UtcNow;
        var now = DateTime.UtcNow;
        var processed = 0;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var studentName = Required(studentRow, "name");
        var student = await Upsert(db.Students, studentName, entity =>
        {
            entity.StudentCode = studentName;
            entity.FirstNames = Join(Optional(studentRow, "first_name"), Optional(studentRow, "middle_name"));
            entity.LastNames = Optional(studentRow, "last_name");
            entity.FullName = Optional(studentRow, "student_name");
            entity.Email = email;
            entity.Phone = Optional(studentRow, "student_mobile_number");
            entity.BirthDate = Date(studentRow, "date_of_birth");
            entity.Gender = Optional(studentRow, "gender");
            entity.Status = Optional(studentRow, "enabled") == "1" ? "active" : "inactive";
            entity.SyncedAt = now;
        }, cancellationToken);
        processed++;

        var periods = new Dictionary<string, AcademicPeriod>();
        foreach (var row in Rows(snapshot, "terms"))
        {
            var name = Required(row, "name");
            periods[name] = await Upsert(db.AcademicPeriods, name, entity =>
            {
                entity.Code = Optional(row, "term_name");
                entity.Name = name;
                entity.StartDate = Date(row, "term_start_date");
                entity.EndDate = Date(row, "term_end_date");
                entity.Status = "active";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        var programs = new Dictionary<string, Program>();
        foreach (var row in Rows(snapshot, "programs"))
        {
            var name = Required(row, "name");
            programs[name] = await Upsert(db.Programs, name, entity =>
            {
                entity.Name = name;
                entity.Code = Optional(row, "program_abbreviation");
                entity.Description = Optional(row, "short_description");
                entity.Status = "active";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        var courses = new Dictionary<string, Course>();
        foreach (var row in Rows(snapshot, "courses"))
        {
            var name = Required(row, "name");
            courses[name] = await Upsert(db.Courses, name, entity =>
            {
                entity.Name = name;
                entity.Code = Optional(row, "course_abbreviation");
                entity.Description = Optional(row, "short_description");
                entity.Credits = int.TryParse(Optional(row, "course_credit"), out var credits) ? credits : null;
                entity.Status = "active";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        var instructors = new Dictionary<string, Instructor>();
        foreach (var row in Rows(snapshot, "faculty"))
        {
            var name = Required(row, "name");
            instructors[name] = await Upsert(db.Instructors, name, entity =>
            {
                entity.FirstName = Optional(row, "first_name");
                entity.LastName = Optional(row, "last_name");
                entity.FullName = Optional(row, "faculty_name") ??
                    Join(Optional(row, "first_name"), Optional(row, "last_name"));
                entity.Email = Optional(row, "email_address");
                entity.Phone = Optional(row, "phone_number");
                entity.Status = Optional(row, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        await db.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "courses"))
        {
            var courseName = Required(row, "name");
            var programName = Optional(row, "program");
            if (programName is null || !programs.TryGetValue(programName, out var program)) continue;
            var course = courses[courseName];
            if (!await db.ProgramCourses.AnyAsync(x => x.ProgramId == program.ProgramId &&
                    x.CourseId == course.CourseId, cancellationToken))
                db.ProgramCourses.Add(new ProgramCourse
                {
                    ProgramId = program.ProgramId, CourseId = course.CourseId, IsRequired = true
                });
        }

        var batchTerms = Rows(snapshot, "enrollments")
            .Where(x => Optional(x, "student_batch") is not null && Optional(x, "academic_term") is not null)
            .ToDictionary(x => Required(x, "student_batch"), x => Required(x, "academic_term"));
        var sections = new Dictionary<string, CourseSection>();
        foreach (var row in Rows(snapshot, "batches"))
        {
            var name = Required(row, "name");
            var course = courses[Required(row, "course")];
            var period = periods[batchTerms[name]];
            sections[name] = await Upsert(db.CourseSections, name, entity =>
            {
                entity.CourseId = course.CourseId;
                entity.AcademicPeriodId = period.AcademicPeriodId;
                entity.SectionCode = name;
                entity.Capacity = int.TryParse(Optional(row, "max_strength"), out var capacity) ? capacity : null;
                entity.Status = Optional(row, "disabled") == "1" ? "inactive" : "active";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        await db.SaveChangesAsync(cancellationToken);

        var seenProgramEnrollments = new HashSet<(Guid ProgramId, Guid PeriodId)>();
        foreach (var row in Rows(snapshot, "enrollments"))
        {
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            await Upsert(db.SectionEnrollments, name, entity =>
            {
                entity.StudentId = student.StudentId;
                entity.SectionId = section.SectionId;
                entity.EnrollmentDate = Date(row, "enrollment_date");
                entity.Status = Optional(row, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            var programName = Optional(row, "program");
            var termName = Optional(row, "academic_term");
            if (programName is not null && termName is not null &&
                programs.TryGetValue(programName, out var program) && periods.TryGetValue(termName, out var period) &&
                seenProgramEnrollments.Add((program.ProgramId, period.AcademicPeriodId)))
            {
                var programEnrollment = await db.ProgramEnrollments.FirstOrDefaultAsync(x =>
                    x.StudentId == student.StudentId && x.ProgramId == program.ProgramId &&
                    x.AcademicPeriodId == period.AcademicPeriodId, cancellationToken);
                if (programEnrollment is null)
                {
                    db.ProgramEnrollments.Add(new ProgramEnrollment
                    {
                        StudentId = student.StudentId, ProgramId = program.ProgramId,
                        AcademicPeriodId = period.AcademicPeriodId,
                        EnrollmentDate = Date(row, "enrollment_date"), Status = "active", SyncedAt = now
                    });
                }
            }
            processed++;
        }
        await db.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "schedules"))
        {
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            var facultyName = Optional(row, "faculty");
            section.InstructorName = Optional(row, "faculty_name");
            await Upsert(db.CourseSchedules, name, entity =>
            {
                entity.SectionId = section.SectionId;
                entity.InstructorId = facultyName is not null && instructors.TryGetValue(facultyName, out var instructor)
                    ? instructor.InstructorId : null;
                entity.ClassDate = Date(row, "schedule_date") ?? throw new InvalidOperationException("Schedule date is missing.");
                entity.StartTime = Time(row, "from_time");
                entity.EndTime = Time(row, "to_time");
                entity.Location = Optional(row, "room");
                entity.Status = "scheduled";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        var assessments = new Dictionary<string, Assessment>();
        var assessmentPlans = new Dictionary<string, AssessmentPlan>();
        foreach (var row in Rows(snapshot, "plans"))
        {
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            var title = Optional(row, "assessment_name") ?? Optional(row, "subject") ?? name;
            var scheduledDate = Date(row, "schedule_date");
            var max = Number(row, "maximum_assessment_score");
            assessmentPlans[name] = await Upsert(db.AssessmentPlans, name, entity =>
            {
                entity.SectionId = section.SectionId;
                entity.Title = title;
                entity.Description = Optional(row, "subject");
                entity.ScheduledDate = scheduledDate;
                entity.MaximumScore = max;
                entity.Status = "scheduled";
                entity.SyncedAt = now;
            }, cancellationToken);
            assessments[name] = await Upsert(db.Assessments, name, entity =>
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
        await db.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "results"))
        {
            var planName = Required(row, "assessment_plan");
            if (!assessments.TryGetValue(planName, out var assessment))
                throw new InvalidOperationException($"Assessment plan {planName} is missing from the snapshot.");
            assessment.Status = "graded";
            assessmentPlans[planName].Status = "completed";
            await Upsert(db.AssessmentResults, Required(row, "name"), entity =>
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
            var section = sections[Required(row, "student_batch")];
            await Upsert(db.AttendanceRecords, Required(row, "name"), entity =>
            {
                entity.StudentId = student.StudentId;
                entity.SectionId = section.SectionId;
                entity.AttendanceDate = Date(row, "date") ?? throw new InvalidOperationException("Attendance date is missing.");
                entity.Status = Required(row, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        var fees = new Dictionary<string, TuitionFee>();
        foreach (var row in Rows(snapshot, "invoices"))
        {
            var name = Required(row, "name");
            fees[name] = await Upsert(db.TuitionFees, name, entity =>
            {
                entity.StudentId = student.StudentId;
                entity.Description = $"Factura {name}";
                entity.CurrencyCode = Optional(row, "currency") ?? "PEN";
                entity.OriginalAmount = Number(row, "grand_total") ?? 0;
                entity.OutstandingBalance = Number(row, "outstanding_amount");
                entity.IssuedOn = Date(row, "posting_date");
                entity.DueDate = Date(row, "due_date");
                entity.Status = Optional(row, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        await db.SaveChangesAsync(cancellationToken);

        var paymentRows = Rows(snapshot, "payments").ToDictionary(x => Required(x, "name"));
        foreach (var row in Rows(snapshot, "payment_references"))
        {
            var paymentName = Required(row, "parent");
            var invoiceName = Required(row, "reference_name");
            if (!paymentRows.TryGetValue(paymentName, out var payment) ||
                !fees.TryGetValue(invoiceName, out var fee)) continue;
            await Upsert(db.Payments, $"{paymentName}:{invoiceName}", entity =>
            {
                entity.StudentId = student.StudentId;
                entity.TuitionFeeId = fee.TuitionFeeId;
                entity.CurrencyCode = fee.CurrencyCode;
                entity.Amount = Number(row, "allocated_amount") ?? 0;
                entity.PaidOn = Date(payment, "posting_date");
                entity.Reference = Optional(payment, "reference_no");
                entity.Status = Optional(payment, "status");
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        if (!await db.UserStudents.AnyAsync(x => x.UserId == userId && x.StudentId == student.StudentId,
                cancellationToken))
            db.UserStudents.Add(new UserStudent
            {
                UserId = userId, StudentId = student.StudentId,
                IsPrimary = !await db.UserStudents.AnyAsync(x => x.UserId == userId && x.IsPrimary,
                    cancellationToken)
            });

        var run = new SyncRun
        {
            EntityName = "student_snapshot", StartDate = startedAt, EndDate = DateTime.UtcNow,
            Status = SyncRunStatus.Completed, ProcessedRecords = processed
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new FrappeSyncSummary(run.SyncRunId, processed);
    }

    private async Task<T> Upsert<T>(DbSet<T> set, string externalId, Action<T> update,
        CancellationToken cancellationToken) where T : class, new()
    {
        var entity = await set.FirstOrDefaultAsync(x => EF.Property<string>(x, "FrappeExternalId") == externalId,
            cancellationToken);
        if (entity is null)
        {
            entity = new T();
            set.Add(entity);
            db.Entry(entity).Property("FrappeExternalId").CurrentValue = externalId;
        }
        update(entity);
        return entity;
    }

    private static IReadOnlyList<JsonElement> Rows(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().ToArray() : [];

    private static string Required(JsonElement row, string property) =>
        Optional(row, property) ?? throw new InvalidOperationException($"Frappe field {property} is missing.");

    private static string? Optional(JsonElement row, string property) =>
        row.TryGetProperty(property, out var value) && value.ValueKind is not
            (JsonValueKind.Null or JsonValueKind.Undefined) ? value.ToString() : null;

    private static DateOnly? Date(JsonElement row, string property)
    {
        var value = Optional(row, property);
        return value is { Length: >= 10 } && DateOnly.TryParseExact(value[..10], "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }

    private static TimeOnly Time(JsonElement row, string property) =>
        TimeOnly.TryParse(Optional(row, property), CultureInfo.InvariantCulture, out var time)
            ? time : throw new InvalidOperationException($"Frappe time {property} is invalid.");

    private static decimal? Number(JsonElement row, string property) =>
        decimal.TryParse(Optional(row, property), NumberStyles.Number, CultureInfo.InvariantCulture,
            out var number) ? number : null;

    private static string Join(params string?[] parts) =>
        string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}

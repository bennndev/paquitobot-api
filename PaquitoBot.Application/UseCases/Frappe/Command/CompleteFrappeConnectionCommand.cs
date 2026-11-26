using System.Globalization;
using System.Text.Json;
using MediatR;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Constants;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.UseCases.Frappe.Command;

public record CompleteFrappeConnectionCommand(string Code, string State) : IRequest<FrappeConnectionResponse>;

internal sealed class CompleteFrappeConnectionCommandHandler(
    IUnitOfWork unitOfWork, IFrappeOAuthService oauth, IFrappeStudentSyncService sync, ITokenService tokenService)
    : IRequestHandler<CompleteFrappeConnectionCommand, FrappeConnectionResponse>
{
    public async Task<FrappeConnectionResponse> Handle(CompleteFrappeConnectionCommand request, CancellationToken cancellationToken)
    {
        var result = await oauth.CompleteAsync(request.Code, request.State, cancellationToken);
        var user = await unitOfWork.Users.GetByIdAsync(result.UserId, cancellationToken);
        if (user is null || !user.IsActive ||
            !string.Equals(user.Email, result.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("The Frappe account does not match the PaquitoBot user.");

        var snapshot = await sync.FetchSnapshotAsync(result.AccessToken, cancellationToken);
        var persister = new FrappeStudentSnapshotPersister(unitOfWork);
        async Task<FrappeSyncSummary> PersistWithinTransaction(CancellationToken token)
        {
            return await persister.PersistAsync(snapshot, user.UserId, result.Email, token);
        }

        var summary = await unitOfWork.ExecuteInTransactionAsync(PersistWithinTransaction, cancellationToken);
        var studentId = await unitOfWork.UserStudents.GetPrimaryStudentIdAsync(user.UserId, cancellationToken);
        var token = tokenService.GenerateToken(user.UserId, user.Role, user.Email, studentId);
        return new FrappeConnectionResponse(user.UserId, result.Email, summary, token);
    }
}

internal sealed class FrappeStudentSnapshotPersister(IUnitOfWork unitOfWork)
{
    public async Task<FrappeSyncSummary> PersistAsync(JsonElement snapshot, Guid userId, string email,
        CancellationToken cancellationToken)
    {
        var studentRow = snapshot.GetProperty("student");
        if (!string.Equals(Required(studentRow, "email_address"), email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Frappe returned a different student's records.");

        return await PersistSnapshotAsync(snapshot, studentRow, userId, email, cancellationToken);
    }

    private async Task<FrappeSyncSummary> PersistSnapshotAsync(JsonElement snapshot, JsonElement studentRow,
        Guid userId, string email, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;
        var now = DateTime.UtcNow;
        var processed = 0;

        var studentName = Required(studentRow, "name");
        var student = await Upsert(unitOfWork.Repository<Student>(), studentName, entity =>
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
            periods[name] = await Upsert(unitOfWork.Repository<AcademicPeriod>(), name, entity =>
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
            programs[name] = await Upsert(unitOfWork.Repository<Program>(), name, entity =>
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
            courses[name] = await Upsert(unitOfWork.Repository<Course>(), name, entity =>
            {
                entity.Name = name;
                entity.Code = Optional(row, "course_abbreviation");
                entity.Description = Optional(row, "short_description");
                entity.Credits = Integer(row, "course_credit");
                entity.Status = "active";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }

        var instructors = new Dictionary<string, Instructor>();
        foreach (var row in Rows(snapshot, "faculty"))
        {
            var name = Required(row, "name");
            instructors[name] = await Upsert(unitOfWork.Repository<Instructor>(), name, entity =>
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "courses"))
        {
            var courseName = Required(row, "name");
            var programName = Optional(row, "program");
            if (programName is null || !programs.TryGetValue(programName, out var program))
            {
                continue;
            }
            var course = courses[courseName];
            if (!await unitOfWork.Repository<ProgramCourse>().AnyAsync(x => x.ProgramId == program.ProgramId &&
                    x.CourseId == course.CourseId, cancellationToken))
                await unitOfWork.Repository<ProgramCourse>().AddAsync(new ProgramCourse
                {
                    ProgramId = program.ProgramId, CourseId = course.CourseId, IsRequired = true
                }, cancellationToken);
        }

        var batchTerms = new Dictionary<string, string>();
        foreach (var enrollment in Rows(snapshot, "enrollments"))
        {
            var batchName = Optional(enrollment, "student_batch");
            var termName = Optional(enrollment, "academic_term");
            if (batchName is not null && termName is not null)
            {
                batchTerms.Add(batchName, termName);
            }
        }
        var sections = new Dictionary<string, CourseSection>();
        foreach (var row in Rows(snapshot, "batches"))
        {
            var name = Required(row, "name");
            var course = courses[Required(row, "course")];
            var period = periods[batchTerms[name]];
            sections[name] = await Upsert(unitOfWork.Repository<CourseSection>(), name, entity =>
            {
                entity.CourseId = course.CourseId;
                entity.AcademicPeriodId = period.AcademicPeriodId;
                entity.SectionCode = name;
                entity.Capacity = Integer(row, "max_strength");
                entity.Status = Optional(row, "disabled") == "1" ? "inactive" : "active";
                entity.SyncedAt = now;
            }, cancellationToken);
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var seenProgramEnrollments = new HashSet<(Guid ProgramId, Guid PeriodId)>();
        foreach (var row in Rows(snapshot, "enrollments"))
        {
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            await Upsert(unitOfWork.Repository<SectionEnrollment>(), name, entity =>
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
                var programEnrollment = await unitOfWork.Repository<ProgramEnrollment>().FirstOrDefaultAsync(x =>
                    x.StudentId == student.StudentId && x.ProgramId == program.ProgramId &&
                    x.AcademicPeriodId == period.AcademicPeriodId, cancellationToken);
                if (programEnrollment is null)
                {
                    await unitOfWork.Repository<ProgramEnrollment>().AddAsync(new ProgramEnrollment
                    {
                        StudentId = student.StudentId, ProgramId = program.ProgramId,
                        AcademicPeriodId = period.AcademicPeriodId,
                        EnrollmentDate = Date(row, "enrollment_date"), Status = "active", SyncedAt = now
                    }, cancellationToken);
                }
            }
            processed++;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var row in Rows(snapshot, "schedules"))
        {
            var name = Required(row, "name");
            var section = sections[Required(row, "student_batch")];
            var facultyName = Optional(row, "faculty");
            section.InstructorName = Optional(row, "faculty_name");
            await Upsert(unitOfWork.Repository<CourseSchedule>(), name, entity =>
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

        var fees = new Dictionary<string, TuitionFee>();
        foreach (var row in Rows(snapshot, "invoices"))
        {
            var name = Required(row, "name");
            fees[name] = await Upsert(unitOfWork.Repository<TuitionFee>(), name, entity =>
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var paymentRows = new Dictionary<string, JsonElement>();
        foreach (var payment in Rows(snapshot, "payments"))
        {
            paymentRows.Add(Required(payment, "name"), payment);
        }
        foreach (var row in Rows(snapshot, "payment_references"))
        {
            var paymentName = Required(row, "parent");
            var invoiceName = Required(row, "reference_name");
            if (!paymentRows.TryGetValue(paymentName, out var payment) ||
                !fees.TryGetValue(invoiceName, out var fee))
            {
                continue;
            }
            await Upsert(unitOfWork.Repository<Payment>(), $"{paymentName}:{invoiceName}", entity =>
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

        if (!await unitOfWork.UserStudents.PairExistsAsync(userId, student.StudentId,
                cancellationToken))
            await unitOfWork.UserStudents.AddAsync(new UserStudent
            {
                UserId = userId, StudentId = student.StudentId,
                IsPrimary = !await unitOfWork.UserStudents.UserHasPrimaryAsync(userId, cancellationToken: cancellationToken)
            }, cancellationToken);

        var run = new SyncRun
        {
            EntityName = "student_snapshot", StartDate = startedAt, EndDate = DateTime.UtcNow,
            Status = SyncRunStatus.Completed, ProcessedRecords = processed
        };
        await unitOfWork.SyncRuns.AddAsync(run, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new FrappeSyncSummary(run.SyncRunId, processed);
    }

    private static Task<T> Upsert<T>(IGenericRepository<T> repository, string externalId, Action<T> update,
        CancellationToken cancellationToken) where T : class
    {
        return repository.UpsertByFrappeExternalIdAsync(externalId, update, cancellationToken);
    }

    private static IReadOnlyList<JsonElement> Rows(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray().ToArray();
    }

    private static string Required(JsonElement row, string property)
    {
        var value = Optional(row, property);
        if (value is null)
        {
            throw new InvalidOperationException($"Frappe field {property} is missing.");
        }

        return value;
    }

    private static string? Optional(JsonElement row, string property)
    {
        if (!row.TryGetProperty(property, out var value) ||
            value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ToString();
    }

    private static DateOnly? Date(JsonElement row, string property)
    {
        var value = Optional(row, property);
        if (value is null || value.Length < 10)
        {
            return null;
        }

        if (DateOnly.TryParseExact(value[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
        {
            return date;
        }

        return null;
    }

    private static TimeOnly Time(JsonElement row, string property)
    {
        if (TimeOnly.TryParse(Optional(row, property), CultureInfo.InvariantCulture, out var time))
        {
            return time;
        }

        throw new InvalidOperationException($"Frappe time {property} is invalid.");
    }

    private static decimal? Number(JsonElement row, string property)
    {
        if (decimal.TryParse(Optional(row, property), NumberStyles.Number, CultureInfo.InvariantCulture,
                out var number))
        {
            return number;
        }

        return null;
    }

    private static int? Integer(JsonElement row, string property)
    {
        if (int.TryParse(Optional(row, property), out var number))
        {
            return number;
        }

        return null;
    }

    private static string Join(params string?[] parts)
    {
        var nonEmptyParts = parts.Where(part => !string.IsNullOrWhiteSpace(part));
        return string.Join(" ", nonEmptyParts);
    }
}

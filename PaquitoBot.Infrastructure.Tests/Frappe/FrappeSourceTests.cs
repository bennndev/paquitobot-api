using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using PaquitoBot.Application.Integrations.Frappe;
using PaquitoBot.Infrastructure.Integrations.Frappe;
using PaquitoBot.Infrastructure.Services.Frappe;

namespace PaquitoBot.Infrastructure.Tests.Frappe;

public class FrappeSourceTests
{
    private const string EmptyPage = """{"data":[]}""";

    private static bool IsFirstPage(HttpRequestMessage request) =>
        request.RequestUri!.Query.Contains("limit_start=0", StringComparison.Ordinal);

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(IsFirstPage(request) ? body : EmptyPage, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class SequenceHandler(Queue<string> pages) : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(pages.Dequeue(), Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class RoutingHandler(Dictionary<string, string> bodiesByDocType) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = Uri.UnescapeDataString(request.RequestUri!.AbsolutePath);
            var body = bodiesByDocType.First(pair => path.EndsWith("/" + pair.Key)).Value;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(IsFirstPage(request) ? body : EmptyPage, Encoding.UTF8, "application/json")
            });
        }
    }

    private static IFrappeClient CreateClient(HttpMessageHandler handler)
    {
        var options = new FrappeOptions
        {
            BaseUrl = "http://frappe.test:8000",
            ApiKey = "test-key",
            ApiSecret = "test-secret"
        };
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://frappe.test:8000/") };
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("token", "test-key:test-secret");

        return new FrappeClient(httpClient, Options.Create(options));
    }

    [Fact]
    public async Task StudentSource_MapsFrappePayload_IncludingSpaceSeparatedTimestamp()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"EDU-STU-2026-00001","first_name":"Ana","last_name":"Lopez","student_name":"Ana Lopez",
            "student_email_id":"ana@mail.com","student_mobile_number":"999","date_of_birth":"2000-05-01",
            "gender":"Female","enabled":1,"modified":"2026-11-24 10:15:30.123456"}]}
            """);

        var students = await new FrappeStudentSource(CreateClient(handler)).GetAllAsync();

        var student = Assert.Single(students);
        Assert.Equal("EDU-STU-2026-00001", student.ExternalId);
        Assert.Equal("ana@mail.com", student.Email);
        Assert.Equal(new DateOnly(2000, 5, 1), student.BirthDate);
        Assert.Equal("Active", student.Status);
        Assert.Equal(new DateTime(2026, 11, 24, 10, 15, 30, 123, DateTimeKind.Utc).AddTicks(4560), student.ModifiedAt);
    }

    [Fact]
    public async Task StudentSource_DisabledStudent_IsMappedAsInactive()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            """{"data":[{"name":"STU-2","first_name":"Luis","enabled":0,"modified":null}]}""");

        var students = await new FrappeStudentSource(CreateClient(handler)).GetAllAsync();

        Assert.Equal("Inactive", Assert.Single(students).Status);
    }

    [Fact]
    public async Task Source_SendsTokenAuthorizationAndRequestsStablyOrderedPages()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """{"data":[]}""");

        await new FrappeStudentSource(CreateClient(handler)).GetAllAsync();

        var request = handler.LastRequest!;
        Assert.Equal("token", request.Headers.Authorization!.Scheme);
        Assert.Equal("test-key:test-secret", request.Headers.Authorization.Parameter);
        Assert.Contains("api/resource/Student", request.RequestUri!.AbsoluteUri);
        Assert.Contains("limit_start=", request.RequestUri.Query);
        Assert.Contains("order_by=name%20asc", request.RequestUri.Query);
    }

    [Fact]
    public async Task Source_ReadsEveryPageUntilFrappeReturnsAnEmptyOne()
    {
        var pages = new Queue<string>(
        [
            """{"data":[{"name":"EDU-STU-1","first_name":"Ana"},{"name":"EDU-STU-2","first_name":"Luis"}]}""",
            """{"data":[{"name":"EDU-STU-3","first_name":"Eva"}]}""",
            EmptyPage
        ]);
        var handler = new SequenceHandler(pages);

        var students = await new FrappeStudentSource(CreateClient(handler)).GetAllAsync();

        Assert.Equal(["EDU-STU-1", "EDU-STU-2", "EDU-STU-3"], students.Select(student => student.ExternalId));
        Assert.Equal(3, handler.Requests);
    }

    [Fact]
    public async Task CourseSource_RoundsFloatCreditsAndUsesAbbreviationAsCode()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"CRS-1","course_name":"Algorithms","course_abbreviation":"ALG",
            "short_description":"Intro","course_credit":3.0,"modified":"2026-01-01 00:00:00"}]}
            """);

        var course = Assert.Single(await new FrappeCourseSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal("ALG", course.Code);
        Assert.Equal("Algorithms", course.Name);
        Assert.Equal(3, course.Credits);
    }

    [Fact]
    public async Task ProgramCourseSource_DerivesPairsFromCourseProgramAndSkipsCoursesWithoutProgram()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            """{"data":[{"name":"CRS-1","program":"PRG-1"},{"name":"CRS-2","program":null},{"name":"CRS-3","program":""}]}""");

        var pair = Assert.Single(await new FrappeProgramCourseSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal("PRG-1", pair.ProgramExternalId);
        Assert.Equal("CRS-1", pair.CourseExternalId);
        Assert.True(pair.IsRequired);
        Assert.Null(pair.CycleNumber);
    }

    [Fact]
    public async Task CourseSectionSource_ReadsStudentBatchNameAndMapsDisabledToInactive()
    {
        var handler = new RoutingHandler(new Dictionary<string, string>
        {
            ["Student Batch Name"] = """
                {"data":[{"name":"BATCH-1","batch_name":"A1","course":"CRS-1","max_strength":30,"disabled":1,
                "start_date":"2026-04-10","end_date":"2026-07-10","modified":"2026-01-01 00:00:00"}]}
                """,
            ["Course Enrollment"] = """{"data":[]}"""
        });

        var section = Assert.Single(await new FrappeCourseSectionSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal("CRS-1", section.CourseExternalId);
        Assert.Equal("A1", section.SectionCode);
        Assert.Equal(30, section.Capacity);
        Assert.Equal("Inactive", section.Status);
        Assert.Equal(new DateOnly(2026, 4, 10), section.StartDate);
        Assert.Null(section.AcademicPeriodExternalId);
    }

    [Fact]
    public async Task CourseSectionSource_TakesTheTermFromTheBatchEnrollments_OnlyWhenTheyAgreeOnOne()
    {
        var handler = new RoutingHandler(new Dictionary<string, string>
        {
            ["Student Batch Name"] = """
                {"data":[
                 {"name":"A1","batch_name":"A1","course":"CRS-1","max_strength":30,"disabled":0,"start_date":"2026-09-03","end_date":null,"modified":null},
                 {"name":"B1","batch_name":"B1","course":"CRS-1","max_strength":30,"disabled":0,"start_date":"2026-09-03","end_date":null,"modified":null},
                 {"name":"C1","batch_name":"C1","course":"CRS-1","max_strength":30,"disabled":0,"start_date":"2026-09-03","end_date":null,"modified":null},
                 {"name":"D1","batch_name":"D1","course":"CRS-1","max_strength":30,"disabled":0,"start_date":"2026-09-03","end_date":null,"modified":null}
                ]}
                """,
            ["Course Enrollment"] = """
                {"data":[
                 {"student_batch":"A1","academic_term":"2026 (2026-II)","docstatus":1},
                 {"student_batch":"A1","academic_term":"2026 (2026-II)","docstatus":1},
                 {"student_batch":"B1","academic_term":"2026 (2026-I)","docstatus":1},
                 {"student_batch":"B1","academic_term":"2026 (2026-II)","docstatus":1},
                 {"student_batch":"C1","academic_term":"2026 (2026-II)","docstatus":2},
                 {"student_batch":"D1","academic_term":null,"docstatus":1}
                ]}
                """
        });

        var sections = await new FrappeCourseSectionSource(CreateClient(handler)).GetAllAsync();

        Assert.Equal("2026 (2026-II)", sections.Single(s => s.ExternalId == "A1").AcademicPeriodExternalId);
        Assert.Null(sections.Single(s => s.ExternalId == "B1").AcademicPeriodExternalId);
        Assert.Null(sections.Single(s => s.ExternalId == "C1").AcademicPeriodExternalId);
        Assert.Null(sections.Single(s => s.ExternalId == "D1").AcademicPeriodExternalId);
    }

    [Fact]
    public async Task CourseScheduleSource_ParsesUnpaddedTimesAndMapsRoomAndFaculty()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"SCH-1","student_batch":"BATCH-1","faculty":"FAC-1","schedule_date":"2026-04-10",
            "from_time":"9:00:00","to_time":"11:30:00","room":"Room 1","modified":"2026-01-01 00:00:00"}]}
            """);

        var slot = Assert.Single(await new FrappeCourseScheduleSource(CreateClient(handler)).GetAllAsync());

        Assert.Contains("api/resource/Subject%20Schedule", handler.LastRequest!.RequestUri!.AbsoluteUri);
        Assert.Equal("BATCH-1", slot.SectionExternalId);
        Assert.Equal("FAC-1", slot.InstructorExternalId);
        Assert.Equal(new TimeOnly(9, 0), slot.StartTime);
        Assert.Equal(new TimeOnly(11, 30), slot.EndTime);
        Assert.Equal("Room 1", slot.Location);
    }

    [Fact]
    public async Task ProgramEnrollmentSource_ResolvesProgramFromRegisterThenCourseAndSkipsApplicantsWithoutStudent()
    {
        var handler = new RoutingHandler(new Dictionary<string, string>
        {
            ["Student Applicant"] = """
                {"data":[
                 {"name":"APP-1","student":"STU-1","admission_register":"REG-1","course":null,"academic_term":"2026-I","application_date":"2026-03-01","application_status":"Approved","modified":"2026-01-01 00:00:00"},
                 {"name":"APP-2","student":"STU-2","admission_register":"REG-2","course":"CRS-1","academic_term":null,"application_date":null,"application_status":"Applied","modified":null},
                 {"name":"APP-3","student":"STU-3","admission_register":"REG-3","course":null,"academic_term":null,"application_date":null,"application_status":"Applied","modified":null},
                 {"name":"APP-4","student":"STU-4","admission_register":"REG-404","course":null,"academic_term":null,"application_date":null,"application_status":"Applied","modified":null},
                 {"name":"APP-5","student":null,"admission_register":"REG-1","course":null,"academic_term":null,"application_date":null,"application_status":"Applied","modified":null}
                ]}
                """,
            ["Admission Register"] = """
                {"data":[{"name":"REG-1","program":"PRG-1","course":null},{"name":"REG-2","program":null,"course":null},{"name":"REG-3","program":null,"course":"CRS-2"}]}
                """,
            ["Course"] = """
                {"data":[{"name":"CRS-1","program":"PRG-2"},{"name":"CRS-2","program":"PRG-3"}]}
                """
        });

        var enrollments = await new FrappeProgramEnrollmentSource(CreateClient(handler)).GetAllAsync();

        Assert.Equal(4, enrollments.Count);
        Assert.DoesNotContain(enrollments, e => e.ExternalId == "APP-5");
        Assert.Equal("PRG-1", enrollments.Single(e => e.ExternalId == "APP-1").ProgramExternalId);
        Assert.Equal("2026-I", enrollments.Single(e => e.ExternalId == "APP-1").AcademicPeriodExternalId);
        Assert.Equal("PRG-2", enrollments.Single(e => e.ExternalId == "APP-2").ProgramExternalId);
        Assert.Equal("PRG-3", enrollments.Single(e => e.ExternalId == "APP-3").ProgramExternalId);
        Assert.Null(enrollments.Single(e => e.ExternalId == "APP-4").ProgramExternalId);
    }

    [Fact]
    public async Task SectionEnrollmentSource_SkipsCancelledAndBatchlessAndKeepsOneEnrollmentPerStudentAndBatch()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[
             {"name":"CE-1","student":"STU-1","student_batch":"A1","enrollment_date":"2026-08-24","status":"Enrolled","docstatus":1,"modified":"2026-01-01 00:00:00"},
             {"name":"CE-2","student":"STU-1","student_batch":"A1","enrollment_date":"2026-08-25","status":"Enrolled","docstatus":1,"modified":null},
             {"name":"CE-3","student":"STU-2","student_batch":"A1","enrollment_date":null,"status":"Enrolled","docstatus":2,"modified":null},
             {"name":"CE-4","student":"STU-3","student_batch":null,"enrollment_date":null,"status":"Enrolled","docstatus":1,"modified":null}
            ]}
            """);

        var enrollments = await new FrappeSectionEnrollmentSource(CreateClient(handler)).GetAllAsync();

        var enrollment = Assert.Single(enrollments);
        Assert.Equal("CE-1", enrollment.ExternalId);
        Assert.Equal("STU-1", enrollment.StudentExternalId);
        Assert.Equal("A1", enrollment.SectionExternalId);
        Assert.Equal(new DateOnly(2026, 8, 24), enrollment.EnrollmentDate);
        Assert.Contains("api/resource/Course%20Enrollment", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task AcademicPeriodSource_MapsTermDates()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"2026 (2026-II)","term_name":"2026-II","term_start_date":"2026-03-01",
            "term_end_date":"2026-07-15","modified":"2026-01-01 00:00:00"}]}
            """);

        var term = Assert.Single(await new FrappeAcademicPeriodSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal("2026 (2026-II)", term.ExternalId);
        Assert.Equal("2026-II", term.Code);
        Assert.Equal("2026 (2026-II)", term.Name);
        Assert.Equal(new DateOnly(2026, 7, 15), term.EndDate);
    }

    [Fact]
    public async Task AcademicPeriodSource_EmptyTermName_LeavesCodeNullAndKeepsIdentifierAsName()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"2026 (2026-II)","term_name":"","term_start_date":null,
            "term_end_date":null,"modified":null}]}
            """);

        var term = Assert.Single(await new FrappeAcademicPeriodSource(CreateClient(handler)).GetAllAsync());

        Assert.Null(term.Code);
        Assert.Equal("2026 (2026-II)", term.Name);
    }

    [Fact]
    public async Task InstructorSource_ReadsFromFacultyDocType()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"FAC-1","first_name":"Rosa","last_name":"Diaz","faculty_name":"Rosa Diaz",
            "email_address":"rosa@mail.com","phone_number":"111","status":"Active","modified":null}]}
            """);

        var instructor = Assert.Single(await new FrappeInstructorSource(CreateClient(handler)).GetAllAsync());

        Assert.Contains("api/resource/Faculty", handler.LastRequest!.RequestUri!.AbsoluteUri);
        Assert.Equal("Rosa Diaz", instructor.FullName);
        Assert.Equal("rosa@mail.com", instructor.Email);
    }

    [Fact]
    public async Task Client_Unauthorized_ThrowsAuthenticationExceptionWithoutLeakingTheSecret()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized, """{"message":"Invalid token"}""");

        var exception = await Assert.ThrowsAsync<FrappeException>(
            () => new FrappeStudentSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal(FrappeFailure.Authentication, exception.Failure);
        Assert.DoesNotContain("test-secret", exception.Message);
        Assert.DoesNotContain("test-key", exception.Message);
    }

    [Fact]
    public async Task Client_ServerError_ThrowsUnavailableFailure()
    {
        var handler = new StubHandler(HttpStatusCode.InternalServerError, "boom");

        var exception = await Assert.ThrowsAsync<FrappeException>(
            () => new FrappeStudentSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal(FrappeFailure.Unavailable, exception.Failure);
    }
}

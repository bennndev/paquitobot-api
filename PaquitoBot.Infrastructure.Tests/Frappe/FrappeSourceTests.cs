using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using PaquitoBot.Infrastructure.Services.Frappe;

namespace PaquitoBot.Infrastructure.Tests.Frappe;

public class FrappeSourceTests
{
    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }

    private static FrappeApiClient CreateClient(StubHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frappe:BaseUrl"] = "http://frappe.test:8000",
                ["Frappe:ApiKey"] = "test-key",
                ["Frappe:ApiSecret"] = "test-secret"
            })
            .Build();

        return new FrappeApiClient(new HttpClient(handler), configuration);
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
    public async Task Source_SendsTokenAuthorizationAndRequestsAllRecords()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """{"data":[]}""");

        await new FrappeStudentSource(CreateClient(handler)).GetAllAsync();

        var request = handler.LastRequest!;
        Assert.Equal("token", request.Headers.Authorization!.Scheme);
        Assert.Equal("test-key:test-secret", request.Headers.Authorization.Parameter);
        Assert.Contains("api/resource/Student", request.RequestUri!.AbsoluteUri);
        Assert.Contains("limit_page_length=0", request.RequestUri.Query);
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
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"BATCH-1","batch_name":"A1","course":"CRS-1","max_strength":30,"disabled":1,
            "start_date":"2026-04-10","end_date":"2026-07-10","modified":"2026-01-01 00:00:00"}]}
            """);

        var section = Assert.Single(await new FrappeCourseSectionSource(CreateClient(handler)).GetAllAsync());

        Assert.Contains("api/resource/Student%20Batch%20Name", handler.LastRequest!.RequestUri!.AbsoluteUri);
        Assert.Equal("CRS-1", section.CourseExternalId);
        Assert.Equal("A1", section.SectionCode);
        Assert.Equal(30, section.Capacity);
        Assert.Equal("Inactive", section.Status);
        Assert.Equal(new DateOnly(2026, 4, 10), section.StartDate);
    }

    [Fact]
    public async Task AcademicPeriodSource_MapsTermDates()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"data":[{"name":"2026-I","term_name":"2026 Spring","term_start_date":"2026-03-01",
            "term_end_date":"2026-07-15","modified":"2026-01-01 00:00:00"}]}
            """);

        var term = Assert.Single(await new FrappeAcademicPeriodSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal("2026 Spring", term.Name);
        Assert.Equal(new DateOnly(2026, 7, 15), term.EndDate);
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

        var exception = await Assert.ThrowsAsync<FrappeAuthenticationException>(
            () => new FrappeStudentSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal(401, exception.StatusCode);
        Assert.DoesNotContain("test-secret", exception.Message);
        Assert.DoesNotContain("test-key", exception.Message);
    }

    [Fact]
    public async Task Client_ServerError_ThrowsApiException()
    {
        var handler = new StubHandler(HttpStatusCode.InternalServerError, "boom");

        var exception = await Assert.ThrowsAsync<FrappeApiException>(
            () => new FrappeStudentSource(CreateClient(handler)).GetAllAsync());

        Assert.Equal(500, exception.StatusCode);
    }

    [Fact]
    public void Client_WithoutBaseUrl_FailsFast()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<InvalidOperationException>(() => new FrappeApiClient(new HttpClient(), configuration));
    }
}

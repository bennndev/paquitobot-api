using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class SectionEnrollmentSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeSectionEnrollmentDto> _source = new();
    private readonly FakeFrappeSyncedRepository<SectionEnrollment> _enrollments = new();
    private readonly FakeFrappeSyncedRepository<Student> _students = new();
    private readonly FakeFrappeSyncedRepository<CourseSection> _sections = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly Student _student = new() { StudentId = Guid.NewGuid(), FrappeExternalId = "STU-1" };
    private readonly CourseSection _section = new() { SectionId = Guid.NewGuid(), FrappeExternalId = "BATCH-1" };

    public SectionEnrollmentSynchronizerTests()
    {
        _students.Items.Add(_student);
        _sections.Items.Add(_section);
    }

    private SectionEnrollmentSynchronizer CreateSut()
        => new(_source, _enrollments, _students, _sections, _unitOfWork, new FakeTimeProvider());

    private static FrappeSectionEnrollmentDto Enrollment(string id, string student = "STU-1", string section = "BATCH-1", string? status = "Enrolled")
        => new(id, student, section, new DateOnly(2026, 8, 24), status, new DateTime(2026, 1, 1));

    [Fact]
    public async Task SyncAsync_CreatesEnrollmentLinkedToStudentAndSection()
    {
        _source.Items.Add(Enrollment("CE-1"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var enrollment = Assert.Single(_enrollments.Items);
        Assert.Equal(_student.StudentId, enrollment.StudentId);
        Assert.Equal(_section.SectionId, enrollment.SectionId);
        Assert.Equal(new DateOnly(2026, 8, 24), enrollment.EnrollmentDate);
        Assert.Equal("enrolled", enrollment.Status);
    }

    [Theory]
    [InlineData("Enrolled", "enrolled")]
    [InlineData("ENROLLED", "enrolled")]
    [InlineData("  ", null)]
    [InlineData(null, null)]
    public async Task SyncAsync_NormalizesStatusToLowercase(string? status, string? expected)
    {
        _source.Items.Add(Enrollment("CE-1", status: status));

        await CreateSut().SyncAsync();

        Assert.Equal(expected, Assert.Single(_enrollments.Items).Status);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_DoesNotDuplicateEnrollments()
    {
        _source.Items.Add(Enrollment("CE-1"));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_enrollments.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_MissingStudent_FailsOnlyThatEnrollment()
    {
        _source.Items.Add(Enrollment("CE-1", student: "STU-404"));
        _source.Items.Add(Enrollment("CE-2"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var error = Assert.Single(result.Errors);
        Assert.Equal("CE-1", error.ExternalId);
        Assert.Contains("Student 'STU-404' was not found", error.Message);
    }

    [Fact]
    public async Task SyncAsync_MissingSection_Fails()
    {
        _source.Items.Add(Enrollment("CE-1", section: "BATCH-404"));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_enrollments.Items);
        Assert.Contains("Section 'BATCH-404' was not found", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_StatusChange_UpdatesTheSameEnrollment()
    {
        _source.Items.Add(Enrollment("CE-1", status: "Enrolled"));
        var sut = CreateSut();
        await sut.SyncAsync();
        var original = _enrollments.Items.Single();

        _source.Items[0] = Enrollment("CE-1", status: "Completed") with { ModifiedAt = new DateTime(2026, 5, 1) };
        var result = await sut.SyncAsync();

        Assert.Equal(1, result.Updated);
        Assert.Same(original, _enrollments.Items.Single());
        Assert.Equal("completed", original.Status);
    }

    [Fact]
    public void Order_IsSectionDetailsTier() => Assert.Equal(SyncOrder.SectionDetails, CreateSut().Order);
}

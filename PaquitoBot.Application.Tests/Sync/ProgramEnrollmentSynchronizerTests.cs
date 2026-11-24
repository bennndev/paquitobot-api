using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class ProgramEnrollmentSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeProgramEnrollmentDto> _source = new();
    private readonly FakeFrappeSyncedRepository<ProgramEnrollment> _enrollments = new();
    private readonly FakeFrappeSyncedRepository<Student> _students = new();
    private readonly FakeFrappeSyncedRepository<Program> _programs = new();
    private readonly FakeFrappeSyncedRepository<AcademicPeriod> _periods = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly Student _student = new() { StudentId = Guid.NewGuid(), FrappeExternalId = "STU-1" };
    private readonly Program _program = new() { ProgramId = Guid.NewGuid(), FrappeExternalId = "PRG-1" };
    private readonly AcademicPeriod _period = new() { AcademicPeriodId = Guid.NewGuid(), FrappeExternalId = "2026-I", Name = "Spring" };

    public ProgramEnrollmentSynchronizerTests()
    {
        _students.Items.Add(_student);
        _programs.Items.Add(_program);
        _periods.Items.Add(_period);
    }

    private ProgramEnrollmentSynchronizer CreateSut()
        => new(_source, _enrollments, _students, _programs, _periods, _unitOfWork, new FakeTimeProvider());

    private static FrappeProgramEnrollmentDto Applicant(
        string id, string student = "STU-1", string? program = "PRG-1", string? period = "2026-I", string? status = "Approved")
        => new(id, student, program, period, new DateOnly(2026, 3, 1), status, new DateTime(2026, 1, 1));

    [Fact]
    public async Task SyncAsync_CreatesEnrollmentLinkedToStudentProgramAndPeriod()
    {
        _source.Items.Add(Applicant("APP-1"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var enrollment = Assert.Single(_enrollments.Items);
        Assert.Equal(_student.StudentId, enrollment.StudentId);
        Assert.Equal(_program.ProgramId, enrollment.ProgramId);
        Assert.Equal(_period.AcademicPeriodId, enrollment.AcademicPeriodId);
        Assert.Equal(new DateOnly(2026, 3, 1), enrollment.EnrollmentDate);
        Assert.Equal("Approved", enrollment.Status);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_DoesNotDuplicateEnrollments()
    {
        _source.Items.Add(Applicant("APP-1"));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_enrollments.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_EnrollmentWithoutPeriod_IsCreatedWithNullPeriod()
    {
        _source.Items.Add(Applicant("APP-1", period: null));

        await CreateSut().SyncAsync();

        Assert.Null(Assert.Single(_enrollments.Items).AcademicPeriodId);
    }

    [Fact]
    public async Task SyncAsync_UnresolvedProgram_FailsWithAClearMessage()
    {
        _source.Items.Add(Applicant("APP-1", program: null));
        _source.Items.Add(Applicant("APP-2"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var error = Assert.Single(result.Errors);
        Assert.Equal("APP-1", error.ExternalId);
        Assert.Contains("program could not be resolved", error.Message);
    }

    [Fact]
    public async Task SyncAsync_MissingStudent_FailsOnlyThatEnrollment()
    {
        _source.Items.Add(Applicant("APP-1", student: "STU-404"));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_enrollments.Items);
        Assert.Contains("Student 'STU-404' was not found", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_MissingProgramRecord_Fails()
    {
        _source.Items.Add(Applicant("APP-1", program: "PRG-404"));

        var result = await CreateSut().SyncAsync();

        Assert.Contains("Program 'PRG-404' was not found", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_UnknownPeriod_Fails()
    {
        _source.Items.Add(Applicant("APP-1", period: "2099-X"));

        var result = await CreateSut().SyncAsync();

        Assert.Contains("Academic period '2099-X' was not found", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_StatusChange_UpdatesTheSameEnrollment()
    {
        _source.Items.Add(Applicant("APP-1", status: "Applied"));
        var sut = CreateSut();
        await sut.SyncAsync();
        var original = _enrollments.Items.Single();

        _source.Items[0] = Applicant("APP-1", status: "Admitted") with { ModifiedAt = new DateTime(2026, 5, 1) };
        var result = await sut.SyncAsync();

        Assert.Equal(1, result.Updated);
        Assert.Same(original, _enrollments.Items.Single());
        Assert.Equal("Admitted", original.Status);
    }

    [Fact]
    public void Order_IsSectionDetailsTier() => Assert.Equal(SyncOrder.SectionDetails, CreateSut().Order);
}

using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class CourseScheduleSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeCourseScheduleDto> _source = new();
    private readonly FakeFrappeSyncedRepository<CourseSchedule> _schedules = new();
    private readonly FakeFrappeSyncedRepository<CourseSection> _sections = new();
    private readonly FakeFrappeSyncedRepository<Instructor> _instructors = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly CourseSection _section = new() { SectionId = Guid.NewGuid(), FrappeExternalId = "BATCH-1" };
    private readonly Instructor _instructor = new() { InstructorId = Guid.NewGuid(), FrappeExternalId = "FAC-1" };

    public CourseScheduleSynchronizerTests()
    {
        _sections.Items.Add(_section);
        _instructors.Items.Add(_instructor);
    }

    private CourseScheduleSynchronizer CreateSut()
        => new(_source, _schedules, _sections, _instructors, _unitOfWork, new FakeTimeProvider());

    private static FrappeCourseScheduleDto Slot(
        string id, string section = "BATCH-1", string? instructor = "FAC-1",
        TimeOnly? start = null, TimeOnly? end = null)
        => new(id, section, instructor, new DateOnly(2026, 4, 10),
            start ?? new TimeOnly(9, 0), end ?? new TimeOnly(11, 0), " Room 1 ", null, null, new DateTime(2026, 1, 1));

    [Fact]
    public async Task SyncAsync_CreatesScheduleLinkedToSectionAndInstructor()
    {
        _source.Items.Add(Slot("SCH-1"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var schedule = Assert.Single(_schedules.Items);
        Assert.Equal(_section.SectionId, schedule.SectionId);
        Assert.Equal(_instructor.InstructorId, schedule.InstructorId);
        Assert.Equal("Room 1", schedule.Location);
        Assert.Equal(new TimeOnly(9, 0), schedule.StartTime);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_DoesNotDuplicateSchedules()
    {
        _source.Items.Add(Slot("SCH-1"));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_schedules.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_ScheduleWithoutInstructor_IsCreatedWithNullInstructor()
    {
        _source.Items.Add(Slot("SCH-1", instructor: null));

        await CreateSut().SyncAsync();

        Assert.Null(Assert.Single(_schedules.Items).InstructorId);
    }

    [Fact]
    public async Task SyncAsync_MissingSection_FailsOnlyThatSchedule()
    {
        _source.Items.Add(Slot("SCH-1", section: "BATCH-404"));
        _source.Items.Add(Slot("SCH-2"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var error = Assert.Single(result.Errors);
        Assert.Equal("SCH-1", error.ExternalId);
        Assert.Contains("Section 'BATCH-404' was not found", error.Message);
    }

    [Fact]
    public async Task SyncAsync_UnknownInstructor_FailsThatSchedule()
    {
        _source.Items.Add(Slot("SCH-1", instructor: "FAC-404"));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_schedules.Items);
        Assert.Contains("Instructor 'FAC-404' was not found", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_EndNotAfterStart_FailsThatScheduleInsteadOfTheWholeBatch()
    {
        _source.Items.Add(Slot("SCH-1", start: new TimeOnly(11, 0), end: new TimeOnly(9, 0)));
        _source.Items.Add(Slot("SCH-2"));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        Assert.Contains("end time", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void Order_IsSectionDetailsTier() => Assert.Equal(SyncOrder.SectionDetails, CreateSut().Order);
}

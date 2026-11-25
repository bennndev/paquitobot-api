using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class CourseSectionSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeCourseSectionDto> _source = new();
    private readonly FakeFrappeSyncedRepository<CourseSection> _sections = new();
    private readonly FakeFrappeSyncedRepository<Course> _courses = new();
    private readonly FakeFrappeSyncedRepository<AcademicPeriod> _periods = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly Course _course = new() { CourseId = Guid.NewGuid(), FrappeExternalId = "CRS-1" };
    private readonly AcademicPeriod _spring = new()
    {
        AcademicPeriodId = Guid.NewGuid(), FrappeExternalId = "2026-I", Name = "Spring",
        StartDate = new DateOnly(2026, 3, 1), EndDate = new DateOnly(2026, 7, 31)
    };

    public CourseSectionSynchronizerTests()
    {
        _courses.Items.Add(_course);
        _periods.Items.Add(_spring);
    }

    private CourseSectionSynchronizer CreateSut()
        => new(_source, _sections, _courses, _periods, _unitOfWork, new FakeTimeProvider());

    private static FrappeCourseSectionDto Batch(string id, string course, DateOnly? start, string? disabledStatus = "Active")
        => new(id, course, start, null, " A1 ", null, null, 30, disabledStatus, new DateTime(2026, 1, 1));

    [Fact]
    public async Task SyncAsync_CreatesSectionLinkedToCourseAndPeriodContainingTheBatchStart()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10)));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var section = Assert.Single(_sections.Items);
        Assert.Equal(_course.CourseId, section.CourseId);
        Assert.Equal(_spring.AcademicPeriodId, section.AcademicPeriodId);
        Assert.Equal("A1", section.SectionCode);
        Assert.Equal(30, section.Capacity);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_DoesNotDuplicateSections()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_sections.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_MissingCourse_FailsOnlyThatSection()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-404", new DateOnly(2026, 4, 10)));
        _source.Items.Add(Batch("BATCH-2", "CRS-1", new DateOnly(2026, 4, 10)));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var error = Assert.Single(result.Errors);
        Assert.Equal("BATCH-1", error.ExternalId);
        Assert.Contains("Course 'CRS-404' was not found", error.Message);
    }

    [Fact]
    public async Task SyncAsync_BatchWithExplicitTerm_UsesItEvenWhenSeveralPeriodsContainTheStart()
    {
        _periods.Items.Add(new AcademicPeriod
        {
            AcademicPeriodId = Guid.NewGuid(), FrappeExternalId = "2026-2027", Name = "Overlap",
            StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31)
        });
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10)) with { AcademicPeriodExternalId = " 2026-I " });

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        Assert.Equal(_spring.AcademicPeriodId, Assert.Single(_sections.Items).AcademicPeriodId);
    }

    [Fact]
    public async Task SyncAsync_ExplicitTermThatIsNotSynced_FailsInsteadOfGuessingByDate()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10)) with { AcademicPeriodExternalId = "2099-X" });

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_sections.Items);
        Assert.Contains("Academic period '2099-X' was not found", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_ExplicitTermDoesNotNeedAStartDate()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", null) with { AcademicPeriodExternalId = "2026-I" });

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
    }

    [Fact]
    public async Task SyncAsync_NoPeriodContainsTheBatchStart_FailsWithAClearMessage()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2027, 1, 10)));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_sections.Items);
        Assert.Contains("No academic period contains", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_MoreThanOnePeriodContainsTheBatchStart_FailsAsAmbiguous()
    {
        _periods.Items.Add(new AcademicPeriod
        {
            AcademicPeriodId = Guid.NewGuid(), FrappeExternalId = "2026-II", Name = "Overlap",
            StartDate = new DateOnly(2026, 4, 1), EndDate = new DateOnly(2026, 9, 30)
        });
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10)));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_sections.Items);
        Assert.Contains("More than one academic period", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_BatchWithoutStartDate_Fails()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", null));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_sections.Items);
        Assert.Contains("start date", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SyncAsync_ModifiedBatch_UpdatesTheSameSection()
    {
        _source.Items.Add(Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10)));
        var sut = CreateSut();
        await sut.SyncAsync();
        var original = _sections.Items.Single();

        _source.Items[0] = Batch("BATCH-1", "CRS-1", new DateOnly(2026, 4, 10), "Inactive") with
        {
            ModifiedAt = new DateTime(2026, 5, 1)
        };
        var result = await sut.SyncAsync();

        Assert.Equal(1, result.Updated);
        Assert.Same(original, _sections.Items.Single());
        Assert.Equal("Inactive", original.Status);
    }

    [Fact]
    public void Order_IsSectionsTier() => Assert.Equal(SyncOrder.Sections, CreateSut().Order);
}

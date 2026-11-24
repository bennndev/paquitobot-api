using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class ProgramCourseSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeProgramCourseDto> _source = new();
    private readonly FakeFrappeSyncedRepository<Program> _programs = new();
    private readonly FakeFrappeSyncedRepository<Course> _courses = new();
    private readonly FakeGenericRepository<ProgramCourse> _links = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly Program _program = new() { ProgramId = Guid.NewGuid(), FrappeExternalId = "PRG-1" };
    private readonly Course _course = new() { CourseId = Guid.NewGuid(), FrappeExternalId = "CRS-1" };

    public ProgramCourseSynchronizerTests()
    {
        _programs.Items.Add(_program);
        _courses.Items.Add(_course);
    }

    private ProgramCourseSynchronizer CreateSut() => new(_source, _programs, _courses, _links, _unitOfWork);

    [Fact]
    public async Task SyncAsync_CreatesTheLinkBetweenExistingProgramAndCourse()
    {
        _source.Items.Add(new FrappeProgramCourseDto("PRG-1", "CRS-1", null, true));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var link = Assert.Single(_links.Items);
        Assert.Equal(_program.ProgramId, link.ProgramId);
        Assert.Equal(_course.CourseId, link.CourseId);
        Assert.True(link.IsRequired);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_DoesNotDuplicateTheLink()
    {
        _source.Items.Add(new FrappeProgramCourseDto("PRG-1", "CRS-1", null, true));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_links.Items);
        Assert.Equal(0, second.Created);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_DuplicatedPairInTheSameBatch_CreatesItOnce()
    {
        _source.Items.Add(new FrappeProgramCourseDto("PRG-1", "CRS-1", null, true));
        _source.Items.Add(new FrappeProgramCourseDto("PRG-1", "CRS-1", null, true));

        var result = await CreateSut().SyncAsync();

        Assert.Single(_links.Items);
        Assert.Equal(1, result.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_MissingCourse_FailsThatLinkAndKeepsTheOthers()
    {
        _source.Items.Add(new FrappeProgramCourseDto("PRG-1", "CRS-404", null, true));
        _source.Items.Add(new FrappeProgramCourseDto("PRG-1", "CRS-1", null, true));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var error = Assert.Single(result.Errors);
        Assert.Equal("CRS-404", error.ExternalId);
        Assert.Contains("not found", error.Message);
    }

    [Fact]
    public async Task SyncAsync_MissingProgram_FailsThatLink()
    {
        _source.Items.Add(new FrappeProgramCourseDto("PRG-404", "CRS-1", null, true));

        var result = await CreateSut().SyncAsync();

        Assert.Empty(_links.Items);
        Assert.Contains("PRG-404", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public void Order_IsProgramStructureTier() => Assert.Equal(SyncOrder.ProgramStructure, CreateSut().Order);
}

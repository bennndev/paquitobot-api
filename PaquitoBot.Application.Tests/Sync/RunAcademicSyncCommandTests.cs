using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Application.UseCases.Sync.Command;
using PaquitoBot.Domain.Constants;

namespace PaquitoBot.Application.Tests.Sync;

public class RunAcademicSyncCommandTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeSender _sender = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _time = new(Now);
    private readonly List<string> _executionLog = [];

    private Task<AcademicSyncResponse> RunAsync(params IEntitySynchronizer[] synchronizers)
        => new RunAcademicSyncCommandHandler(synchronizers, _sender, _unitOfWork, _time)
            .Handle(new RunAcademicSyncCommand(), CancellationToken.None);

    private FakeEntitySynchronizer Fake(string name, int order, SyncResult? result = null, Exception? failure = null)
        => new(name, order, _executionLog, result, failure);

    [Fact]
    public async Task Handle_RunsSynchronizersInDependencyOrder()
    {
        await RunAsync(
            Fake("CourseSchedule", SyncOrder.SectionDetails),
            Fake("Student", SyncOrder.MasterData),
            Fake("CourseSection", SyncOrder.Sections));

        Assert.Equal(["Student", "CourseSection", "CourseSchedule"], _executionLog);
    }

    [Fact]
    public async Task Handle_RecordsOneCompletedSyncRunPerEntityWithProcessedCount()
    {
        var response = await RunAsync(
            Fake("Student", SyncOrder.MasterData, new SyncResult("Student", 2, 1, 4, [])));

        var start = Assert.Single(_sender.Started);
        Assert.Equal("Student", start.EntityName);
        var (_, completion) = Assert.Single(_sender.Completed);
        Assert.True(completion.Succeeded);
        Assert.Equal(7, completion.ProcessedRecords);
        Assert.Null(completion.ErrorMessage);

        var entity = Assert.Single(response.Entities);
        Assert.Equal(SyncRunStatus.Completed, entity.Status);
        Assert.Equal(2, entity.Created);
        Assert.Equal(1, entity.Updated);
        Assert.Equal(4, entity.Unchanged);
    }

    [Fact]
    public async Task Handle_RecordFailures_MarkTheRunFailedWithUsefulDetailsAndCounts()
    {
        var errors = new List<SyncError> { new("STU-9", "Course 'CRS-404' was not found.") };

        var response = await RunAsync(
            Fake("Student", SyncOrder.MasterData, new SyncResult("Student", 3, 0, 0, errors)));

        var (_, completion) = Assert.Single(_sender.Completed);
        Assert.False(completion.Succeeded);
        Assert.Contains("STU-9", completion.ErrorMessage);
        Assert.Contains("Course 'CRS-404' was not found.", completion.ErrorMessage);
        Assert.Contains("created=3", completion.ErrorMessage);
        Assert.Contains("failed=1", completion.ErrorMessage);
        Assert.Equal(4, completion.ProcessedRecords);

        var entity = Assert.Single(response.Entities);
        Assert.Equal(SyncRunStatus.Failed, entity.Status);
        Assert.Equal(1, entity.Failed);
    }

    [Fact]
    public async Task Handle_ErrorListInTheRunMessageIsBounded()
    {
        var errors = Enumerable.Range(1, 50).Select(i => new SyncError($"STU-{i}", "boom")).ToList();

        await RunAsync(Fake("Student", SyncOrder.MasterData, new SyncResult("Student", 0, 0, 0, errors)));

        var message = Assert.Single(_sender.Completed).Command.ErrorMessage!;
        Assert.Contains("STU-1:", message);
        Assert.DoesNotContain("STU-50:", message);
        Assert.Contains("failed=50", message);
    }

    [Fact]
    public async Task Handle_EntityThrows_IsRecordedAsFailed_DiscardsChanges_AndContinuesWithTheRest()
    {
        var response = await RunAsync(
            Fake("Student", SyncOrder.MasterData, failure: new InvalidOperationException("Frappe is unreachable")),
            Fake("Course", SyncOrder.MasterData));

        Assert.Equal(["Student", "Course"], _executionLog);
        Assert.Equal(1, _unitOfWork.DiscardCount);
        Assert.Equal(2, _sender.Completed.Count);

        var failed = response.Entities.Single(e => e.EntityName == "Student");
        Assert.Equal(SyncRunStatus.Failed, failed.Status);
        Assert.Equal(1, failed.Failed);
        Assert.Equal(failed.Errors.Count, failed.Failed);
        Assert.Equal(0, failed.Processed + failed.Created + failed.Updated + failed.Unchanged);
        Assert.Equal(1, response.Failed);
        Assert.Contains("Frappe is unreachable", _sender.Completed[0].Command.ErrorMessage);
        Assert.Equal(0, _sender.Completed[0].Command.ProcessedRecords);
        Assert.Equal(SyncRunStatus.Completed, response.Entities.Single(e => e.EntityName == "Course").Status);
    }

    [Fact]
    public async Task Handle_ReportsTotalsAcrossEntities()
    {
        var response = await RunAsync(
            Fake("Student", SyncOrder.MasterData, new SyncResult("Student", 2, 1, 0, [])),
            Fake("Course", SyncOrder.MasterData, new SyncResult("Course", 1, 0, 3, [new SyncError("C-1", "x")])));

        Assert.Equal(3, response.Created);
        Assert.Equal(1, response.Updated);
        Assert.Equal(3, response.Unchanged);
        Assert.Equal(1, response.Failed);
        Assert.Equal(8, response.Processed);
        Assert.All(response.Entities, e => Assert.Equal(e.Errors.Count, e.Failed));
    }

    [Fact]
    public async Task Handle_TotalsIncludeAWholeEntityFailure()
    {
        var response = await RunAsync(
            Fake("Student", SyncOrder.MasterData, failure: new InvalidOperationException("403")),
            Fake("Course", SyncOrder.MasterData, new SyncResult("Course", 1, 0, 3, [new SyncError("C-1", "x")])));

        Assert.Equal(2, response.Failed);
        Assert.All(response.Entities, e => Assert.Equal(e.Errors.Count, e.Failed));
    }

    [Fact]
    public async Task Handle_ARecentRunIsStillRunning_RejectsTheNewSyncWithoutStartingAnything()
    {
        _sender.ExistingRuns.Add(FakeSender.RunningSince(Now.UtcDateTime.AddMinutes(-5)));

        await Assert.ThrowsAsync<ConflictException>(() => RunAsync(Fake("Student", SyncOrder.MasterData)));

        Assert.Empty(_executionLog);
        Assert.Empty(_sender.Started);
    }

    [Fact]
    public async Task Handle_AStaleRunningRecord_DoesNotBlockANewSync()
    {
        _sender.ExistingRuns.Add(FakeSender.RunningSince(Now.UtcDateTime.AddHours(-3)));

        await RunAsync(Fake("Student", SyncOrder.MasterData));

        Assert.Equal(["Student"], _executionLog);
    }
}

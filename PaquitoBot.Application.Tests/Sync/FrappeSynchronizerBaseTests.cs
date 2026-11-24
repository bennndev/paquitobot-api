using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class FrappeSynchronizerBaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeFrappeSyncedRepository<Student> _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _time = new(Now);
    private readonly List<FrappeStudentDto> _source = [];

    private TestStudentSynchronizer CreateSut() => new(_repository, _unitOfWork, _time, _source);

    private static FrappeStudentDto Dto(string id, string name, DateTime? modifiedAt)
        => new(id, null, null, null, name, null, null, null, null, null, modifiedAt);

    [Fact]
    public async Task SyncAsync_CreatesMissingRecords()
    {
        _source.Add(Dto("STU-1", "Ana", new DateTime(2026, 1, 1)));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var student = Assert.Single(_repository.Items);
        Assert.Equal("STU-1", student.FrappeExternalId);
        Assert.Equal(new DateTime(2026, 1, 1), student.FrappeModifiedAt);
        Assert.Equal(Now.UtcDateTime, student.SyncedAt);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_DoesNotCreateDuplicates()
    {
        _source.Add(Dto("STU-1", "Ana", new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(0, second.Created);
        Assert.Equal(0, second.Updated);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_ExternalIdWithSpaces_DoesNotCreateDuplicates()
    {
        _source.Add(Dto(" STU-1 ", "Ana", new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_UpdatesOnlyModifiedRecords()
    {
        _source.Add(Dto("STU-1", "Ana", new DateTime(2026, 1, 1)));
        _source.Add(Dto("STU-2", "Luis", new DateTime(2026, 1, 1)));
        var sut = CreateSut();
        await sut.SyncAsync();

        _source[0] = Dto("STU-1", "Ana Maria", new DateTime(2026, 2, 1));
        _time.Advance(TimeSpan.FromHours(1));
        var result = await sut.SyncAsync();

        Assert.Equal(1, result.Updated);
        Assert.Equal(1, result.Unchanged);
        var ana = _repository.Items.Single(s => s.FrappeExternalId == "STU-1");
        var luis = _repository.Items.Single(s => s.FrappeExternalId == "STU-2");
        Assert.Equal("Ana Maria", ana.FullName);
        Assert.Equal(Now.AddHours(1).UtcDateTime, ana.SyncedAt);
        Assert.Equal(Now.UtcDateTime, luis.SyncedAt);
    }

    [Fact]
    public async Task SyncAsync_RecordFailure_IsReportedAndOthersContinue()
    {
        _source.Add(Dto("", "Broken", null));
        _source.Add(Dto("STU-2", "Luis", null));

        var result = await CreateSut().SyncAsync();

        Assert.Equal(1, result.Created);
        var error = Assert.Single(result.Errors);
        Assert.Equal(1, result.Failed);
        Assert.Contains("external id", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SyncAsync_SavesOnce()
    {
        _source.Add(Dto("STU-1", "Ana", null));
        _source.Add(Dto("STU-2", "Luis", null));

        await CreateSut().SyncAsync();

        Assert.Equal(1, _unitOfWork.SaveCount);
    }
}

internal sealed class TestStudentSynchronizer(
    FakeFrappeSyncedRepository<Student> repository,
    FakeUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    List<FrappeStudentDto> source)
    : FrappeSynchronizerBase<FrappeStudentDto, Student>(repository, unitOfWork, timeProvider)
{
    public override string EntityName => "Student";

    public override int Order => 1;

    protected override Task<IReadOnlyList<FrappeStudentDto>> FetchAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<FrappeStudentDto>>(source.ToList());

    protected override string GetExternalId(FrappeStudentDto external) => external.ExternalId;

    protected override DateTime? GetModifiedAt(FrappeStudentDto external) => external.ModifiedAt;

    protected override Student Create(FrappeStudentDto external)
        => new() { FrappeExternalId = external.ExternalId.Trim(), FullName = external.FullName };

    protected override void Apply(FrappeStudentDto external, Student entity) => entity.FullName = external.FullName;
}

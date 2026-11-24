using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class CourseSynchronizerTests
{
    private readonly FakeFrappeClient _client = new();
    private readonly FakeFrappeSyncedRepository<Course> _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CourseSynchronizer CreateSut() => new(_client, _repository, _unitOfWork, new FakeTimeProvider());

    [Fact]
    public async Task SyncAsync_MapsAndNormalizesFields()
    {
        _client.Courses.Add(new FrappeCourseDto(" EXT-1 ", " C1 ", " Algorithms ", null, 4, "Active", new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        var entity = Assert.Single(_repository.Items);
        Assert.Equal("EXT-1", entity.FrappeExternalId);
        Assert.Equal("C1", entity.Code);
        Assert.Equal("Algorithms", entity.Name);
        Assert.Equal(4, entity.Credits);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_KeepsASingleRecord()
    {
        _client.Courses.Add(new FrappeCourseDto(" EXT-1 ", " C1 ", " Algorithms ", null, 4, "Active", new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public void Order_IsMasterDataTier() => Assert.Equal(SyncOrder.MasterData, CreateSut().Order);
}

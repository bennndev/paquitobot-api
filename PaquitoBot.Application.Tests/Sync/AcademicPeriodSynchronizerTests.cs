using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class AcademicPeriodSynchronizerTests
{
    private readonly FakeFrappeClient _client = new();
    private readonly FakeFrappeSyncedRepository<AcademicPeriod> _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AcademicPeriodSynchronizer CreateSut() => new(_client, _repository, _unitOfWork, new FakeTimeProvider());

    [Fact]
    public async Task SyncAsync_MapsAndNormalizesFields()
    {
        _client.AcademicPeriods.Add(new FrappeAcademicPeriodDto(" EXT-1 ", " 2026-1 ", " Spring ", new DateOnly(2026, 3, 1), new DateOnly(2026, 7, 1), "Open", new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        var entity = Assert.Single(_repository.Items);
        Assert.Equal("EXT-1", entity.FrappeExternalId);
        Assert.Equal("2026-1", entity.Code);
        Assert.Equal("Spring", entity.Name);
        Assert.Equal(new DateOnly(2026, 7, 1), entity.EndDate);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_KeepsASingleRecord()
    {
        _client.AcademicPeriods.Add(new FrappeAcademicPeriodDto(" EXT-1 ", " 2026-1 ", " Spring ", new DateOnly(2026, 3, 1), new DateOnly(2026, 7, 1), "Open", new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public void Order_IsMasterDataTier() => Assert.Equal(SyncOrder.MasterData, CreateSut().Order);
}

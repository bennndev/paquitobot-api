using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class ProgramSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeProgramDto> _source = new();
    private readonly FakeFrappeSyncedRepository<Program> _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ProgramSynchronizer CreateSut() => new(_source, _repository, _unitOfWork, new FakeTimeProvider());

    [Fact]
    public async Task SyncAsync_MapsAndNormalizesFields()
    {
        _source.Items.Add(new FrappeProgramDto(" EXT-1 ", " P1 ", " Software ", " Desc ", "Active", new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        var entity = Assert.Single(_repository.Items);
        Assert.Equal("EXT-1", entity.FrappeExternalId);
        Assert.Equal("P1", entity.Code);
        Assert.Equal("Software", entity.Name);
        Assert.Equal("Desc", entity.Description);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_KeepsASingleRecord()
    {
        _source.Items.Add(new FrappeProgramDto(" EXT-1 ", " P1 ", " Software ", " Desc ", "Active", new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public void Order_IsMasterDataTier() => Assert.Equal(SyncOrder.MasterData, CreateSut().Order);
}

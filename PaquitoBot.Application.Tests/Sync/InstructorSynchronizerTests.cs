using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class InstructorSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeInstructorDto> _source = new();
    private readonly FakeFrappeSyncedRepository<Instructor> _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private InstructorSynchronizer CreateSut() => new(_source, _repository, _unitOfWork, new FakeTimeProvider());

    [Fact]
    public async Task SyncAsync_MapsAndNormalizesFields()
    {
        _source.Items.Add(new FrappeInstructorDto(" EXT-1 ", " Ana ", " Lopez ", " Ana Lopez ", " ANA@Mail.com ", null, "Active", new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        var entity = Assert.Single(_repository.Items);
        Assert.Equal("EXT-1", entity.FrappeExternalId);
        Assert.Equal("Ana Lopez", entity.FullName);
        Assert.Equal("ana@mail.com", entity.Email);
    }

    [Theory]
    [InlineData("Active", "active")]
    [InlineData("ACTIVE", "active")]
    [InlineData("  ", null)]
    [InlineData(null, null)]
    public async Task SyncAsync_NormalizesStatusToLowercase(string? status, string? expected)
    {
        _source.Items.Add(new FrappeInstructorDto("EXT-1", "Ana", "Lopez", "Ana Lopez", null, null, status, new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        Assert.Equal(expected, Assert.Single(_repository.Items).Status);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_KeepsASingleRecord()
    {
        _source.Items.Add(new FrappeInstructorDto(" EXT-1 ", " Ana ", " Lopez ", " Ana Lopez ", " ANA@Mail.com ", null, "Active", new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public void Order_IsMasterDataTier() => Assert.Equal(SyncOrder.MasterData, CreateSut().Order);
}

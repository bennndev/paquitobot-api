using Microsoft.Extensions.Time.Testing;
using PaquitoBot.Application.DTOs.Frappe;
using PaquitoBot.Application.Services.Sync;
using PaquitoBot.Application.Tests.Fakes;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Tests.Sync;

public class StudentSynchronizerTests
{
    private readonly FakeFrappeSource<FrappeStudentDto> _source = new();
    private readonly FakeFrappeSyncedRepository<Student> _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private StudentSynchronizer CreateSut() => new(_source, _repository, _unitOfWork, new FakeTimeProvider());

    [Fact]
    public async Task SyncAsync_MapsAndNormalizesStudentFields()
    {
        _source.Items.Add(new FrappeStudentDto(
            " STU-1 ", " S001 ", " Ana ", " Lopez ", null, " ANA@Mail.com ", null,
            new DateOnly(2000, 5, 1), "F", "Active", new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        var student = Assert.Single(_repository.Items);
        Assert.Equal("STU-1", student.FrappeExternalId);
        Assert.Equal("S001", student.StudentCode);
        Assert.Equal("Ana Lopez", student.FullName);
        Assert.Equal("ana@mail.com", student.Email);
        Assert.Equal(new DateOnly(2000, 5, 1), student.BirthDate);
    }

    [Theory]
    [InlineData("Active", "active")]
    [InlineData("ACTIVE", "active")]
    [InlineData("  ", null)]
    [InlineData(null, null)]
    public async Task SyncAsync_NormalizesStatusToLowercase(string? status, string? expected)
    {
        _source.Items.Add(new FrappeStudentDto("STU-1", null, "Ana", null, null, null, null, null, null, status, new DateTime(2026, 1, 1)));

        await CreateSut().SyncAsync();

        Assert.Equal(expected, Assert.Single(_repository.Items).Status);
    }

    [Fact]
    public async Task SyncAsync_RunTwice_KeepsASingleStudent()
    {
        _source.Items.Add(new FrappeStudentDto("STU-1", null, "Ana", null, null, null, null, null, null, null, new DateTime(2026, 1, 1)));
        var sut = CreateSut();

        await sut.SyncAsync();
        var second = await sut.SyncAsync();

        Assert.Single(_repository.Items);
        Assert.Equal(1, second.Unchanged);
    }

    [Fact]
    public async Task SyncAsync_ModifiedStudent_UpdatesTheSameRecord()
    {
        _source.Items.Add(new FrappeStudentDto("STU-1", null, "Ana", null, null, null, null, null, null, "Active", new DateTime(2026, 1, 1)));
        var sut = CreateSut();
        await sut.SyncAsync();
        var original = _repository.Items.Single();

        _source.Items[0] = new FrappeStudentDto("STU-1", null, "Ana", null, null, null, null, null, null, "Inactive", new DateTime(2026, 3, 1));
        var result = await sut.SyncAsync();

        Assert.Equal(1, result.Updated);
        Assert.Same(original, _repository.Items.Single());
        Assert.Equal("inactive", original.Status);
    }

    [Fact]
    public void Order_IsMasterDataTier()
    {
        Assert.Equal(SyncOrder.MasterData, CreateSut().Order);
    }
}

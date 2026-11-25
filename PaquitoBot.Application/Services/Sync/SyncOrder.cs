namespace PaquitoBot.Application.Services.Sync;

/// <summary>
/// Dependency tiers for a sync run: each tier only references entities from lower tiers.
/// </summary>
public static class SyncOrder
{
    public const int MasterData = 10;

    public const int ProgramStructure = 20;

    public const int Sections = 30;

    public const int SectionDetails = 40;
}

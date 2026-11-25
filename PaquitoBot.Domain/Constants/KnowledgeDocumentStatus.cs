namespace PaquitoBot.Domain.Constants;

public static class KnowledgeDocumentStatus
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Indexed = "indexed";
    public const string Failed = "failed";

    public static readonly IReadOnlyList<string> All = [Pending, Processing, Indexed, Failed];
}

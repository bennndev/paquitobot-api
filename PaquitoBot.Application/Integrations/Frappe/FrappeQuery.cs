namespace PaquitoBot.Application.Integrations.Frappe;

public sealed record FrappeQuery(
    string Resource,
    IReadOnlyList<string>? Fields = null,
    IReadOnlyList<FrappeFilter>? Filters = null,
    int Offset = 0,
    int PageSize = 100,
    string? ParentDocType = null);

public sealed record FrappeFilter(string Field, string Operator, object? Value);

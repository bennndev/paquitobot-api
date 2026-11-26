namespace PaquitoBot.Application.Dtos.Frappe;

public sealed record FrappeQuery(
    string Resource,
    IReadOnlyList<string>? Fields = null,
    IReadOnlyList<FrappeFilter>? Filters = null,
    int Offset = 0,
    int PageSize = 100);

public sealed record FrappeFilter(string Field, string Operator, object? Value);

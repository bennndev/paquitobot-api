namespace PaquitoBot.Application.Dtos.Frappe;

public sealed record FrappePage<T>(IReadOnlyList<T> Items, int? NextOffset);

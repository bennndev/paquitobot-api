namespace PaquitoBot.Application.Integrations.Frappe;

public sealed record FrappePage<T>(IReadOnlyList<T> Items, int? NextOffset);

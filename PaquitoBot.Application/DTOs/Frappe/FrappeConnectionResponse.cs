using PaquitoBot.Application.Services;

namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeConnectionResponse(Guid UserId, string Email, FrappeSyncSummary Sync, string Token);

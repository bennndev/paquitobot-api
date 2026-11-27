using PaquitoBot.Application.Services;
using PaquitoBot.Application.Services.SyncByStudent;

namespace PaquitoBot.Application.DTOs.Frappe;

public record FrappeConnectionResponse(Guid UserId, string Email, FrappeSyncSummary Sync, string Token);

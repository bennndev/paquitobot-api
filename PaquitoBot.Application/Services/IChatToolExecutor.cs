namespace PaquitoBot.Application.Services;

public interface IChatToolExecutor
{
    Task<string> ExecuteAsync(string toolName, CancellationToken cancellationToken = default);
}

namespace PaquitoBot.Application.Services.ChatBot;

public interface IChatToolExecutor
{
    Task<string> ExecuteAsync(string toolName, CancellationToken cancellationToken = default);
}

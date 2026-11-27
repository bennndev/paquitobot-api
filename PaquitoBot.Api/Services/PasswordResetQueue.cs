        using System.Threading.Channels;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Api.Services;

internal sealed class PasswordResetQueue(
    IServiceScopeFactory scopeFactory, ILogger<PasswordResetQueue> logger) : BackgroundService, IPasswordResetQueue
{
    private readonly Channel<string> _requests = Channel.CreateBounded<string>(100);

    public bool TryEnqueue(string email) => _requests.Writer.TryWrite(email);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in _requests.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var delivery = scope.ServiceProvider.GetRequiredService<IPasswordResetDelivery>();
                var user = await users.GetByEmailAsync(email, stoppingToken);
                if (user is { IsActive: true, PasswordHash: { Length: > 0 } hash })
                    await delivery.SendAsync(user.Email, delivery.Protect(user.UserId, user.Email, hash), stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Password reset processing failed ({FailureType}).", exception.GetType().Name);
            }
        }
    }
}

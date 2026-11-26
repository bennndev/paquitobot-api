namespace PaquitoBot.Application.Services;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAdministrator { get; }
}

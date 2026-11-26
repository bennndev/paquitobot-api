namespace PaquitoBot.Application.Services;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? StudentId { get; }
    bool IsAdministrator { get; }
}

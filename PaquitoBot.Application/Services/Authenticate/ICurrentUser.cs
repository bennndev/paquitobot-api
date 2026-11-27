namespace PaquitoBot.Application.Services.Authenticate;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? StudentId { get; }
    bool IsAdministrator { get; }
}

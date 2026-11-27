using PaquitoBot.Domain.Models;

namespace PaquitoBot.Application.Services.Authenticate;

public interface IUserPasswordHasher
{
    bool ValidatePassword(User user, string storeHash, string password);
    string HashPassword(User user, string password);
}
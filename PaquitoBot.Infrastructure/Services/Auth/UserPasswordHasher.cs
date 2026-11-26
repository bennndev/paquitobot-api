using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace PaquitoBot.Infrastructure.Services.Auth;

public class UserPasswordHasher : IUserPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User>
        _hasher = new();

    public bool ValidatePassword(User user, string storeHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(user, storeHash,  password);
        return result != PasswordVerificationResult.Failed;
    }

    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }
}
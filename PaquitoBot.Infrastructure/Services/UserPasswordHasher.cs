using PaquitoBot.Application.Services;
using PaquitoBot.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace PaquitoBot.Infrastructure.Services;

public class UserPasswordHasher : IUserPasswordHasher
{
    //Clase de microsoft para hashear y validar contrasenas
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User>
        _hasher = new();

    public bool ValidatePassword(User user, string storeHash, string password)
    {
        //Retorna (Success, Failed o SuccessRehashNeeded)
        var result = _hasher.VerifyHashedPassword(user, storeHash,  password);
        return result != PasswordVerificationResult.Failed;
    }

    //Retorna la contrasena hasheada
    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }
}
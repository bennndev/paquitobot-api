using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PaquitoBot.Application.Services.Authenticate;

namespace PaquitoBot.Infrastructure.Services.Authenticate;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(Guid idUser, string role, string email, Guid? studentId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, idUser.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, email),
        };
        if (studentId.HasValue)
        {
            claims.Add(new Claim("student_id", studentId.Value.ToString()));
        }
        var secret = configuration["JWTVariable:Secret"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWTVariable:Secret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = configuration["JWTVariable:Issuer"];
        var audience = configuration["JWTVariable:Audience"];

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

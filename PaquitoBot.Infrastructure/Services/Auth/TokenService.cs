using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using PaquitoBot.Infrastructure.Configuration;
using Microsoft.IdentityModel.Tokens;
using PaquitoBot.Application.Services;

namespace PaquitoBot.Infrastructure.Services.Auth;

public class TokenService(IOptions<JwtSettings> options) : ITokenService
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
        var settings = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

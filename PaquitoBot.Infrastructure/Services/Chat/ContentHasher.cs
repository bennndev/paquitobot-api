using PaquitoBot.Application.Services;
using System.Security.Cryptography;
using System.Text;

namespace PaquitoBot.Infrastructure.Services.Chat;

internal sealed class ContentHasher : IContentHasher
{
    public string Sha256(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

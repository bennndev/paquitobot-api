using System.Security.Cryptography;
using System.Text;

namespace PaquitoBot.Application.Common.Text;

public static class ContentHasher
{
    public static string Sha256(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

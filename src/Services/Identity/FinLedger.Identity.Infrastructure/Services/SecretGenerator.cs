using System.Security.Cryptography;
using FinLedger.Identity.Application.Abstractions;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class SecretGenerator : ISecretGenerator
{
    public string CreateRefreshToken()
    {
        return ToBase64Url(RandomNumberGenerator.GetBytes(64));
    }

    public string CreateApiKey(string clientId, out string keyPrefix)
    {
        var normalizedClientId = clientId.Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        var clientPrefix = normalizedClientId.Length <= 12 ? normalizedClientId : normalizedClientId[..12];
        var secret = ToBase64Url(RandomNumberGenerator.GetBytes(48));
        var apiKey = $"flk_{clientPrefix}_{secret}";
        keyPrefix = apiKey[..Math.Min(24, apiKey.Length)];
        return apiKey;
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

using System.Security.Cryptography;
using System.Text;
using FinLedger.Identity.Application.Abstractions;
using FinLedger.Identity.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace FinLedger.Identity.Infrastructure.Services;

public sealed class Sha256SecretHasher : ISecretHasher
{
    private readonly IdentitySecurityOptions _options;

    public Sha256SecretHasher(IOptions<IdentitySecurityOptions> options)
    {
        _options = options.Value;
    }

    public string Hash(string secret)
    {
        var bytes = Encoding.UTF8.GetBytes($"{_options.SecretPepper}.{secret}");
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

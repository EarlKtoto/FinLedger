namespace FinLedger.Identity.Application.Abstractions;

public interface ISecretGenerator
{
    string CreateRefreshToken();

    string CreateApiKey(string clientId, out string keyPrefix);
}

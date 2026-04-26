namespace FinLedger.Identity.Application.Abstractions;

public interface ISecretHasher
{
    string Hash(string secret);
}

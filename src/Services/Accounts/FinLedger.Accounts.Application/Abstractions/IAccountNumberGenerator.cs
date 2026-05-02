namespace FinLedger.Accounts.Application.Abstractions;

public interface IAccountNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}

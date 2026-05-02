using FinLedger.BankIntegration.Domain.Entities;

namespace FinLedger.BankIntegration.Application.Abstractions;

public interface IBankIntegrationRequestLogRepository
{
    Task AddAsync(BankIntegrationRequestLog log, CancellationToken cancellationToken = default);
}

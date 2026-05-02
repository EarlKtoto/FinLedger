using FinLedger.BankIntegration.Application.Abstractions;
using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.BankIntegration.Infrastructure.Persistence;

namespace FinLedger.BankIntegration.Infrastructure.Repositories;

public sealed class BankIntegrationRequestLogRepository : IBankIntegrationRequestLogRepository
{
    private readonly BankIntegrationDbContext _dbContext;

    public BankIntegrationRequestLogRepository(BankIntegrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BankIntegrationRequestLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.BankIntegrationRequestLogs.AddAsync(log, cancellationToken);
    }
}

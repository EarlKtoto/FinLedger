using FinLedger.BankIntegration.Application.Abstractions;
using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.BankIntegration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.BankIntegration.Infrastructure.Repositories;

public sealed class BankConnectionRepository : IBankConnectionRepository
{
    private readonly BankIntegrationDbContext _dbContext;

    public BankConnectionRepository(BankIntegrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BankConnection connection, CancellationToken cancellationToken = default)
    {
        await _dbContext.BankConnections.AddAsync(connection, cancellationToken);
    }

    public Task<BankConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.BankConnections.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<BankConnection?> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.BankConnections.FirstOrDefaultAsync(x => x.ParticipantId == participantId, cancellationToken);
    }
}

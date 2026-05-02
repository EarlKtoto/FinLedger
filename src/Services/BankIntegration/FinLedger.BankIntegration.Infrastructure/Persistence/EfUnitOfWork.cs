using FinLedger.BankIntegration.Application.Abstractions;

namespace FinLedger.BankIntegration.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly BankIntegrationDbContext _dbContext;

    public EfUnitOfWork(BankIntegrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

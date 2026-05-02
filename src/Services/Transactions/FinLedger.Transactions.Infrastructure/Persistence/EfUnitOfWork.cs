using FinLedger.Transactions.Application.Abstractions;

namespace FinLedger.Transactions.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly TransactionsDbContext _dbContext;

    public EfUnitOfWork(TransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

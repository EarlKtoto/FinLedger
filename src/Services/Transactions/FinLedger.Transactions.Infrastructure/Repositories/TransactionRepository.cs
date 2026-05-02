using FinLedger.Transactions.Application.Abstractions;
using FinLedger.Transactions.Domain.Entities;
using FinLedger.Transactions.Domain.Enums;
using FinLedger.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Transactions.Infrastructure.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly TransactionsDbContext _dbContext;

    public TransactionRepository(TransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Transaction?> GetByNumberAsync(string transactionNumber, CancellationToken cancellationToken = default)
    {
        return Query().FirstOrDefaultAsync(x => x.TransactionNumber == transactionNumber, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAsync(TransactionStatus? status, Guid? participantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (participantId.HasValue)
        {
            query = query.Where(x => x.PayerParticipantId == participantId.Value || x.ReceiverParticipantId == participantId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Transaction> Query()
    {
        return _dbContext.Transactions.Include(x => x.History);
    }
}

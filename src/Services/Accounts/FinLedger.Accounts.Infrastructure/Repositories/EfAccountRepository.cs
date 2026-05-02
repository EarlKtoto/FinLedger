using FinLedger.Accounts.Application.Abstractions;
using FinLedger.Accounts.Domain.Entities;
using FinLedger.Accounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Accounts.Infrastructure.Repositories;

public sealed class EfAccountRepository : IAccountRepository
{
    private readonly AccountsDbContext _dbContext;

    public EfAccountRepository(AccountsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _dbContext.Accounts.Update(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAccounts()
            .OrderBy(x => x.AccountNumber)
            .ToListAsync(cancellationToken);
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return QueryAccounts().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Account?> GetByNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        return QueryAccounts().FirstOrDefaultAsync(x => x.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Account>> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        return await QueryAccounts()
            .Where(x => x.ParticipantId == participantId)
            .OrderBy(x => x.AccountNumber)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Account> QueryAccounts()
    {
        return _dbContext.Accounts
            .Include(x => x.Limits)
            .Include(x => x.StatusHistory);
    }
}

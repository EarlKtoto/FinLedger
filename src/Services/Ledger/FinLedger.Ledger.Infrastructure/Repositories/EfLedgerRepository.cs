using FinLedger.Ledger.Application.Abstractions;
using FinLedger.Ledger.Domain.Entities;
using FinLedger.Ledger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinLedger.Ledger.Infrastructure.Repositories;

public sealed class EfLedgerRepository : ILedgerRepository
{
    private readonly LedgerDbContext _dbContext;

    public EfLedgerRepository(LedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ILedgerOperationTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return new EfLedgerOperationTransaction(await _dbContext.Database.BeginTransactionAsync(cancellationToken));
    }

    public Task<LedgerAccount?> GetAccountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LedgerAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId, cancellationToken);
    }

    public Task<AccountBalance?> GetBalanceByLedgerAccountIdAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccountBalances
            .Include(x => x.LedgerAccount)
            .FirstOrDefaultAsync(x => x.LedgerAccountId == ledgerAccountId, cancellationToken);
    }

    public Task<AccountBalance?> GetBalanceByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccountBalances
            .Include(x => x.LedgerAccount)
            .FirstOrDefaultAsync(x => x.LedgerAccount != null && x.LedgerAccount.AccountId == accountId, cancellationToken);
    }

    public Task<LedgerTransaction?> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return QueryTransactions().FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken);
    }

    public Task<LedgerTransaction?> GetTransactionByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        return QueryTransactions()
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.ExternalTransactionId == externalTransactionId, cancellationToken);
    }

    public Task<LedgerTransaction?> GetTransactionByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return QueryTransactions().FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task<LedgerTransaction?> GetReversalForTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return QueryTransactions().FirstOrDefaultAsync(x => x.ReversedTransactionId == transactionId, cancellationToken);
    }

    public Task<FundsReservation?> GetReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return QueryReservations().FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
    }

    public Task<FundsReservation?> GetReservationByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
    {
        return QueryReservations()
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.ExternalTransactionId == externalTransactionId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<LedgerEntry>> GetEntriesByAccountIdAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LedgerEntries
            .Include(x => x.LedgerAccount)
            .Where(x => x.LedgerAccount != null && x.LedgerAccount.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddLedgerAccountAsync(LedgerAccount account, CancellationToken cancellationToken = default)
    {
        await _dbContext.LedgerAccounts.AddAsync(account, cancellationToken);
    }

    public async Task AddAccountBalanceAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.AccountBalances.AddAsync(balance, cancellationToken);
    }

    public async Task AddLedgerTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.LedgerTransactions.AddAsync(transaction, cancellationToken);
    }

    public async Task AddReservationAsync(FundsReservation reservation, CancellationToken cancellationToken = default)
    {
        await _dbContext.FundsReservations.AddAsync(reservation, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<LedgerTransaction> QueryTransactions()
    {
        return _dbContext.LedgerTransactions
            .Include(x => x.Entries)
            .ThenInclude(x => x.LedgerAccount);
    }

    private IQueryable<FundsReservation> QueryReservations()
    {
        return _dbContext.FundsReservations.Include(x => x.LedgerAccount);
    }
}

internal sealed class EfLedgerOperationTransaction : ILedgerOperationTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfLedgerOperationTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }
}

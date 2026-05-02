using FinLedger.Ledger.Domain.Entities;

namespace FinLedger.Ledger.Application.Abstractions;

public interface ILedgerRepository
{
    Task<ILedgerOperationTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<LedgerAccount?> GetAccountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<AccountBalance?> GetBalanceByLedgerAccountIdAsync(Guid ledgerAccountId, CancellationToken cancellationToken = default);

    Task<AccountBalance?> GetBalanceByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<LedgerTransaction?> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<LedgerTransaction?> GetTransactionByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);

    Task<LedgerTransaction?> GetTransactionByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task<LedgerTransaction?> GetReversalForTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<FundsReservation?> GetReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);

    Task<FundsReservation?> GetReservationByExternalIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LedgerEntry>> GetEntriesByAccountIdAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken = default);

    Task AddLedgerAccountAsync(LedgerAccount account, CancellationToken cancellationToken = default);

    Task AddAccountBalanceAsync(AccountBalance balance, CancellationToken cancellationToken = default);

    Task AddLedgerTransactionAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default);

    Task AddReservationAsync(FundsReservation reservation, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ILedgerOperationTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}

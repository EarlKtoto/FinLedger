using FinLedger.Transactions.Domain.Entities;
using FinLedger.Transactions.Domain.Enums;

namespace FinLedger.Transactions.Application.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByNumberAsync(string transactionNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Transaction>> GetAsync(TransactionStatus? status, Guid? participantId, int skip, int take, CancellationToken cancellationToken = default);
}

using FinLedger.Accounts.Domain.Entities;

namespace FinLedger.Accounts.Application.Abstractions;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Account>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Account?> GetByNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Account>> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default);
}

using FinLedger.BankIntegration.Domain.Entities;

namespace FinLedger.BankIntegration.Application.Abstractions;

public interface IBankConnectionRepository
{
    Task AddAsync(BankConnection connection, CancellationToken cancellationToken = default);

    Task<BankConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BankConnection?> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default);
}

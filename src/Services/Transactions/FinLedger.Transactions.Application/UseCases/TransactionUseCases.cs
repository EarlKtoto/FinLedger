using FinLedger.Contracts.Transactions;

namespace FinLedger.Transactions.Application.UseCases;

public sealed record CreateTransactionCommand(
    Guid PayerParticipantId,
    Guid ReceiverParticipantId,
    Guid PayerAccountId,
    Guid ReceiverAccountId,
    string PayerBankCode,
    string PayerAccountNumber,
    string ReceiverBankCode,
    string ReceiverAccountNumber,
    decimal Amount,
    string CurrencyCode,
    string? Description,
    string? ExternalReference);

public sealed record ProcessTransactionCommand(Guid TransactionId);

public sealed record CancelTransactionCommand(Guid TransactionId, string? Reason);

public sealed record GetTransactionByIdQuery(Guid TransactionId);

public sealed record GetTransactionByNumberQuery(string TransactionNumber);

public sealed record GetTransactionsQuery(string? Status, Guid? ParticipantId, int Skip, int Take);

public interface ITransactionUseCaseService
{
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionCommand command, CancellationToken cancellationToken = default);

    Task<TransactionDto> ProcessTransactionAsync(ProcessTransactionCommand command, CancellationToken cancellationToken = default);

    Task<TransactionDto> CancelTransactionAsync(CancelTransactionCommand command, CancellationToken cancellationToken = default);

    Task<TransactionDto> GetTransactionByIdAsync(GetTransactionByIdQuery query, CancellationToken cancellationToken = default);

    Task<TransactionDto> GetTransactionByNumberAsync(GetTransactionByNumberQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TransactionDto>> GetTransactionsAsync(GetTransactionsQuery query, CancellationToken cancellationToken = default);
}

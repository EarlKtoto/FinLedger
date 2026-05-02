using FinLedger.Transactions.Domain.Entities;

namespace FinLedger.Transactions.Application.Abstractions;

public interface IBankIntegrationClient
{
    Task<ExternalOperationResult> ValidatePayerAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<ExternalOperationResult> ValidateReceiverAsync(Transaction transaction, CancellationToken cancellationToken = default);
}

public interface ILedgerClient
{
    Task<LedgerReservationResult> ReserveFundsAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<LedgerCommitResult> CommitTransferAsync(Transaction transaction, Guid reservationId, CancellationToken cancellationToken = default);

    Task<ExternalOperationResult> ReverseAsync(Transaction transaction, Guid? reservationId, Guid? ledgerTransactionId, string reason, CancellationToken cancellationToken = default);
}

public interface IAuditClient
{
    Task RecordTransactionEventAsync(Transaction transaction, string operation, string outcome, string details, CancellationToken cancellationToken = default);
}

public sealed record ExternalOperationResult(bool Succeeded, string Message)
{
    public static ExternalOperationResult Success(string message) => new(true, message);

    public static ExternalOperationResult Failure(string message) => new(false, message);
}

public sealed record LedgerReservationResult(bool Succeeded, Guid? ReservationId, string Message)
{
    public static LedgerReservationResult Success(Guid reservationId, string message) => new(true, reservationId, message);

    public static LedgerReservationResult Failure(string message) => new(false, null, message);
}

public sealed record LedgerCommitResult(bool Succeeded, Guid? LedgerTransactionId, string Message)
{
    public static LedgerCommitResult Success(Guid ledgerTransactionId, string message) => new(true, ledgerTransactionId, message);

    public static LedgerCommitResult Failure(string message) => new(false, null, message);
}

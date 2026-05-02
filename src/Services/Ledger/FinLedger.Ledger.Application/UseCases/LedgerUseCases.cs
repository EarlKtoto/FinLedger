using FinLedger.Contracts.Ledger;

namespace FinLedger.Ledger.Application.UseCases;

public sealed record RegisterLedgerAccountCommand(
    Guid AccountId,
    Guid ParticipantId,
    string AccountNumber,
    string CurrencyCode,
    decimal InitialAvailableBalance);

public sealed record GetLedgerAccountQuery(Guid AccountId);

public sealed record GetBalanceQuery(Guid AccountId);

public sealed record ReserveFundsCommand(
    Guid AccountId,
    string ExternalTransactionId,
    string IdempotencyKey,
    decimal Amount,
    string CurrencyCode,
    DateTimeOffset? ExpiresAtUtc,
    string? Description);

public sealed record ReleaseReservationCommand(Guid ReservationId, string IdempotencyKey, string? Description);

public sealed record CaptureReservationCommand(
    Guid ReservationId,
    Guid CreditAccountId,
    string IdempotencyKey,
    string? ExternalTransactionId,
    string? Description);

public sealed record CreateTransferCommand(
    Guid DebitAccountId,
    Guid CreditAccountId,
    string ExternalTransactionId,
    string IdempotencyKey,
    decimal Amount,
    string CurrencyCode,
    string? Description);

public sealed record ReverseLedgerTransactionCommand(
    Guid TransactionId,
    string IdempotencyKey,
    string? ExternalTransactionId,
    string? Description);

public sealed record GetLedgerTransactionQuery(Guid TransactionId);

public sealed record GetLedgerTransactionByExternalIdQuery(string ExternalTransactionId);

public sealed record GetAccountEntriesQuery(Guid AccountId, int Skip, int Take);

public interface ILedgerUseCaseService
{
    Task<LedgerAccountDto> RegisterLedgerAccountAsync(RegisterLedgerAccountCommand command, CancellationToken cancellationToken = default);

    Task<LedgerAccountDto> GetLedgerAccountAsync(GetLedgerAccountQuery query, CancellationToken cancellationToken = default);

    Task<AccountBalanceDto> GetBalanceAsync(GetBalanceQuery query, CancellationToken cancellationToken = default);

    Task<FundsReservationDto> ReserveFundsAsync(ReserveFundsCommand command, CancellationToken cancellationToken = default);

    Task<FundsReservationDto> ReleaseReservationAsync(ReleaseReservationCommand command, CancellationToken cancellationToken = default);

    Task<LedgerTransactionDto> CaptureReservationAsync(CaptureReservationCommand command, CancellationToken cancellationToken = default);

    Task<LedgerTransactionDto> CreateTransferAsync(CreateTransferCommand command, CancellationToken cancellationToken = default);

    Task<LedgerTransactionDto> ReverseLedgerTransactionAsync(ReverseLedgerTransactionCommand command, CancellationToken cancellationToken = default);

    Task<LedgerTransactionDto> GetLedgerTransactionAsync(GetLedgerTransactionQuery query, CancellationToken cancellationToken = default);

    Task<LedgerTransactionDto> GetLedgerTransactionByExternalIdAsync(GetLedgerTransactionByExternalIdQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LedgerEntryDto>> GetAccountEntriesAsync(GetAccountEntriesQuery query, CancellationToken cancellationToken = default);
}

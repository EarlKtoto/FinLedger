using FinLedger.Contracts.Accounts;

namespace FinLedger.Accounts.Application.UseCases;

public sealed record CreateAccountCommand(
    Guid ParticipantId,
    string Type,
    string CurrencyCode,
    string DisplayName,
    bool AllowIncomingPayments,
    bool AllowOutgoingPayments);

public sealed record UpdateAccountCommand(
    Guid AccountId,
    string DisplayName,
    bool AllowIncomingPayments,
    bool AllowOutgoingPayments);

public sealed record ActivateAccountCommand(Guid AccountId);

public sealed record SuspendAccountCommand(Guid AccountId);

public sealed record FreezeAccountCommand(Guid AccountId);

public sealed record CloseAccountCommand(Guid AccountId, string? Reason);

public sealed record GetAccountByIdQuery(Guid AccountId);

public sealed record GetAccountByNumberQuery(string AccountNumber);

public sealed record GetAccountsByParticipantQuery(Guid ParticipantId);

public sealed record ValidateAccountForIncomingPaymentQuery(Guid? AccountId, string? AccountNumber, string CurrencyCode, decimal Amount);

public sealed record ValidateAccountForOutgoingPaymentQuery(Guid? AccountId, string? AccountNumber, string CurrencyCode, decimal Amount);

public sealed record SetAccountLimitCommand(Guid AccountId, Guid? LimitId, string LimitType, decimal Amount);

public sealed record RemoveAccountLimitCommand(Guid AccountId, Guid LimitId);

public interface IAccountUseCaseService
{
    Task<IReadOnlyCollection<AccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default);

    Task<AccountDto> GetAccountByIdAsync(GetAccountByIdQuery query, CancellationToken cancellationToken = default);

    Task<AccountDto> GetAccountByNumberAsync(GetAccountByNumberQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AccountDto>> GetAccountsByParticipantAsync(GetAccountsByParticipantQuery query, CancellationToken cancellationToken = default);

    Task<AccountDto> CreateAccountAsync(CreateAccountCommand command, CancellationToken cancellationToken = default);

    Task<AccountDto> UpdateAccountAsync(UpdateAccountCommand command, CancellationToken cancellationToken = default);

    Task<AccountDto> ActivateAccountAsync(ActivateAccountCommand command, CancellationToken cancellationToken = default);

    Task<AccountDto> SuspendAccountAsync(SuspendAccountCommand command, CancellationToken cancellationToken = default);

    Task<AccountDto> FreezeAccountAsync(FreezeAccountCommand command, CancellationToken cancellationToken = default);

    Task<AccountDto> CloseAccountAsync(CloseAccountCommand command, CancellationToken cancellationToken = default);

    Task<ValidateAccountPaymentResponse> ValidateIncomingAsync(ValidateAccountForIncomingPaymentQuery query, CancellationToken cancellationToken = default);

    Task<ValidateAccountPaymentResponse> ValidateOutgoingAsync(ValidateAccountForOutgoingPaymentQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AccountLimitDto>> GetLimitsAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<AccountLimitDto> SetLimitAsync(SetAccountLimitCommand command, CancellationToken cancellationToken = default);

    Task RemoveLimitAsync(RemoveAccountLimitCommand command, CancellationToken cancellationToken = default);
}

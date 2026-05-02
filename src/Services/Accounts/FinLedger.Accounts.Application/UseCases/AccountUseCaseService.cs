using FinLedger.Accounts.Application.Abstractions;
using FinLedger.Accounts.Application.Exceptions;
using FinLedger.Accounts.Domain.Entities;
using FinLedger.Accounts.Domain.Enums;
using FinLedger.Contracts.Accounts;

namespace FinLedger.Accounts.Application.UseCases;

public sealed class AccountUseCaseService : IAccountUseCaseService
{
    private readonly IAccountRepository _repository;
    private readonly IAccountNumberGenerator _accountNumberGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AccountUseCaseService(
        IAccountRepository repository,
        IAccountNumberGenerator accountNumberGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _accountNumberGenerator = accountNumberGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<AccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _repository.GetAllAsync(cancellationToken);
        return accounts.Select(Map).ToArray();
    }

    public async Task<AccountDto> GetAccountByIdAsync(GetAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        return Map(await GetRequiredByIdAsync(query.AccountId, cancellationToken));
    }

    public async Task<AccountDto> GetAccountByNumberAsync(GetAccountByNumberQuery query, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByNumberAsync(query.AccountNumber, cancellationToken)
            ?? throw new AccountNotFoundException($"Account '{query.AccountNumber}' was not found.");

        return Map(account);
    }

    public async Task<IReadOnlyCollection<AccountDto>> GetAccountsByParticipantAsync(GetAccountsByParticipantQuery query, CancellationToken cancellationToken = default)
    {
        var accounts = await _repository.GetByParticipantIdAsync(query.ParticipantId, cancellationToken);
        return accounts.Select(Map).ToArray();
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ParticipantId == Guid.Empty)
        {
            throw new AccountsValidationException("ParticipantId is required.");
        }

        var type = ParseEnum<AccountType>(command.Type, nameof(command.Type));
        var accountNumber = await _accountNumberGenerator.GenerateAsync(cancellationToken);
        var account = Account.Create(
            command.ParticipantId,
            accountNumber,
            type,
            command.CurrencyCode,
            command.DisplayName,
            command.AllowIncomingPayments,
            command.AllowOutgoingPayments,
            _dateTimeProvider.UtcNow);

        await _repository.AddAsync(account, cancellationToken);
        return Map(account);
    }

    public async Task<AccountDto> UpdateAccountAsync(UpdateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var account = await GetRequiredByIdAsync(command.AccountId, cancellationToken);
        try
        {
            account.Update(command.DisplayName, command.AllowIncomingPayments, command.AllowOutgoingPayments, _dateTimeProvider.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new AccountConflictException(exception.Message);
        }

        await _repository.UpdateAsync(account, cancellationToken);
        return Map(account);
    }

    public Task<AccountDto> ActivateAccountAsync(ActivateAccountCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeStatusAsync(command.AccountId, account => account.Activate(_dateTimeProvider.UtcNow), cancellationToken);
    }

    public Task<AccountDto> SuspendAccountAsync(SuspendAccountCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeStatusAsync(command.AccountId, account => account.Suspend(_dateTimeProvider.UtcNow), cancellationToken);
    }

    public Task<AccountDto> FreezeAccountAsync(FreezeAccountCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeStatusAsync(command.AccountId, account => account.Freeze(_dateTimeProvider.UtcNow), cancellationToken);
    }

    public Task<AccountDto> CloseAccountAsync(CloseAccountCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeStatusAsync(command.AccountId, account => account.Close(_dateTimeProvider.UtcNow, command.Reason), cancellationToken);
    }

    public async Task<ValidateAccountPaymentResponse> ValidateIncomingAsync(ValidateAccountForIncomingPaymentQuery query, CancellationToken cancellationToken = default)
    {
        var account = await FindForValidationAsync(query.AccountId, query.AccountNumber, cancellationToken);
        return Validate(account, query.CurrencyCode, query.Amount, incoming: true);
    }

    public async Task<ValidateAccountPaymentResponse> ValidateOutgoingAsync(ValidateAccountForOutgoingPaymentQuery query, CancellationToken cancellationToken = default)
    {
        var account = await FindForValidationAsync(query.AccountId, query.AccountNumber, cancellationToken);
        return Validate(account, query.CurrencyCode, query.Amount, incoming: false);
    }

    public async Task<IReadOnlyCollection<AccountLimitDto>> GetLimitsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await GetRequiredByIdAsync(accountId, cancellationToken);
        return account.Limits.OrderBy(x => x.LimitType).Select(Map).ToArray();
    }

    public async Task<AccountLimitDto> SetLimitAsync(SetAccountLimitCommand command, CancellationToken cancellationToken = default)
    {
        var account = await GetRequiredByIdAsync(command.AccountId, cancellationToken);
        var limitType = ParseEnum<LimitType>(command.LimitType, nameof(command.LimitType));

        if (command.Amount <= 0)
        {
            throw new AccountsValidationException("Limit amount must be positive.");
        }

        if (command.LimitId.HasValue)
        {
            var existing = account.Limits.FirstOrDefault(x => x.Id == command.LimitId.Value)
                ?? throw new AccountNotFoundException($"Limit '{command.LimitId}' was not found.");

            if (existing.LimitType != limitType)
            {
                throw new AccountsValidationException("Limit type cannot be changed through this endpoint.");
            }
        }

        AccountLimit limit;
        try
        {
            limit = account.SetLimit(limitType, command.Amount, _dateTimeProvider.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new AccountConflictException(exception.Message);
        }

        await _repository.UpdateAsync(account, cancellationToken);
        return Map(limit);
    }

    public async Task RemoveLimitAsync(RemoveAccountLimitCommand command, CancellationToken cancellationToken = default)
    {
        var account = await GetRequiredByIdAsync(command.AccountId, cancellationToken);
        if (account.Limits.All(x => x.Id != command.LimitId))
        {
            throw new AccountNotFoundException($"Limit '{command.LimitId}' was not found.");
        }

        try
        {
            account.RemoveLimit(command.LimitId, _dateTimeProvider.UtcNow);
        }
        catch (InvalidOperationException exception)
        {
            throw new AccountConflictException(exception.Message);
        }

        await _repository.UpdateAsync(account, cancellationToken);
    }

    private async Task<AccountDto> ChangeStatusAsync(Guid accountId, Action<Account> change, CancellationToken cancellationToken)
    {
        var account = await GetRequiredByIdAsync(accountId, cancellationToken);
        try
        {
            change(account);
        }
        catch (InvalidOperationException exception)
        {
            throw new AccountConflictException(exception.Message);
        }

        await _repository.UpdateAsync(account, cancellationToken);
        return Map(account);
    }

    private async Task<Account> GetRequiredByIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(accountId, cancellationToken)
            ?? throw new AccountNotFoundException($"Account '{accountId}' was not found.");
    }

    private async Task<Account?> FindForValidationAsync(Guid? accountId, string? accountNumber, CancellationToken cancellationToken)
    {
        if (accountId.HasValue)
        {
            return await _repository.GetByIdAsync(accountId.Value, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(accountNumber))
        {
            return await _repository.GetByNumberAsync(accountNumber, cancellationToken);
        }

        throw new AccountsValidationException("Either AccountId or AccountNumber must be provided.");
    }

    private static ValidateAccountPaymentResponse Validate(Account? account, string currencyCode, decimal amount, bool incoming)
    {
        if (account is null)
        {
            return new ValidateAccountPaymentResponse(false, null, null, "Account must exist.");
        }

        if (amount <= 0)
        {
            return Invalid(account, "Amount must be positive.");
        }

        if (!string.Equals(account.CurrencyCode, NormalizeCurrency(currencyCode), StringComparison.OrdinalIgnoreCase))
        {
            return Invalid(account, "Currency must match.");
        }

        if (account.Status == AccountStatus.Closed)
        {
            return Invalid(account, "Account must not be Closed.");
        }

        if (account.Status == AccountStatus.Frozen)
        {
            return Invalid(account, "Account must not be Frozen.");
        }

        if (incoming && !account.AllowIncomingPayments)
        {
            return Invalid(account, "Incoming payments are not allowed.");
        }

        if (!incoming && !account.AllowOutgoingPayments)
        {
            return Invalid(account, "Outgoing payments are not allowed.");
        }

        var singleLimitType = incoming ? LimitType.SingleTransactionIncoming : LimitType.SingleTransactionOutgoing;
        var singleLimit = account.Limits.FirstOrDefault(x => x.LimitType == singleLimitType);
        if (singleLimit is not null && amount > singleLimit.Amount)
        {
            return Invalid(account, "Single transaction limit is exceeded.");
        }

        return new ValidateAccountPaymentResponse(true, account.Id, account.AccountNumber, "Account is valid.");
    }

    private static ValidateAccountPaymentResponse Invalid(Account account, string message)
    {
        return new ValidateAccountPaymentResponse(false, account.Id, account.AccountNumber, message);
    }

    private static AccountDto Map(Account account)
    {
        return new AccountDto(
            account.Id,
            account.ParticipantId,
            account.AccountNumber,
            account.Type.ToString(),
            account.Status.ToString(),
            account.CurrencyCode,
            account.DisplayName,
            account.AllowIncomingPayments,
            account.AllowOutgoingPayments,
            account.CreatedAtUtc,
            account.UpdatedAtUtc,
            account.ClosedAtUtc);
    }

    private static AccountLimitDto Map(AccountLimit limit)
    {
        return new AccountLimitDto(
            limit.Id,
            limit.AccountId,
            limit.LimitType.ToString(),
            limit.Amount,
            limit.CurrencyCode,
            limit.CreatedAtUtc,
            limit.UpdatedAtUtc);
    }

    private static TEnum ParseEnum<TEnum>(string value, string parameterName)
        where TEnum : struct
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(typeof(TEnum), parsed))
        {
            return parsed;
        }

        throw new AccountsValidationException($"'{value}' is not a valid {parameterName}.");
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}

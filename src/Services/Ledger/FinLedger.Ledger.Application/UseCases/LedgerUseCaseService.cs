using FinLedger.Contracts.Ledger;
using FinLedger.Ledger.Application.Abstractions;
using FinLedger.Ledger.Application.Exceptions;
using FinLedger.Ledger.Domain.Entities;
using FinLedger.Ledger.Domain.Enums;

namespace FinLedger.Ledger.Application.UseCases;

public sealed class LedgerUseCaseService : ILedgerUseCaseService
{
    private const int MaxEntriesTake = 500;

    private readonly ILedgerRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LedgerUseCaseService(ILedgerRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<LedgerAccountDto> RegisterLedgerAccountAsync(RegisterLedgerAccountCommand command, CancellationToken cancellationToken = default)
    {
        if (command.InitialAvailableBalance < 0)
        {
            throw new LedgerValidationException("InitialAvailableBalance cannot be negative.");
        }

        var currencyCode = NormalizeCurrency(command.CurrencyCode);
        await using var operation = await _repository.BeginTransactionAsync(cancellationToken);

        var existing = await _repository.GetAccountByAccountIdAsync(command.AccountId, cancellationToken);
        if (existing is not null)
        {
            if (existing.ParticipantId != command.ParticipantId ||
                !string.Equals(existing.AccountNumber, command.AccountNumber, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(existing.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new LedgerConflictException($"Ledger account for AccountId '{command.AccountId}' already exists with different metadata.");
            }

            return Map(existing);
        }

        var now = _dateTimeProvider.UtcNow;
        var account = CreateDomain(() => LedgerAccount.Register(command.AccountId, command.ParticipantId, command.AccountNumber, currencyCode, now));
        var balance = CreateDomain(() => AccountBalance.Create(account.Id, command.InitialAvailableBalance, currencyCode, now));

        await _repository.AddLedgerAccountAsync(account, cancellationToken);
        await _repository.AddAccountBalanceAsync(balance, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await operation.CommitAsync(cancellationToken);

        return Map(account);
    }

    public async Task<LedgerAccountDto> GetLedgerAccountAsync(GetLedgerAccountQuery query, CancellationToken cancellationToken = default)
    {
        return Map(await GetRequiredAccountAsync(query.AccountId, cancellationToken));
    }

    public async Task<AccountBalanceDto> GetBalanceAsync(GetBalanceQuery query, CancellationToken cancellationToken = default)
    {
        var balance = await _repository.GetBalanceByAccountIdAsync(query.AccountId, cancellationToken)
            ?? throw new LedgerNotFoundException($"Balance for account '{query.AccountId}' was not found.");

        return Map(balance, balance.LedgerAccount);
    }

    public async Task<FundsReservationDto> ReserveFundsAsync(ReserveFundsCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAmount(command.Amount);
        var currencyCode = NormalizeCurrency(command.CurrencyCode);
        if (command.ExpiresAtUtc.HasValue && command.ExpiresAtUtc <= _dateTimeProvider.UtcNow)
        {
            throw new LedgerValidationException("ExpiresAtUtc must be in the future.");
        }

        await using var operation = await _repository.BeginTransactionAsync(cancellationToken);
        var existingTransaction = await GetIdempotentTransactionAsync(command.IdempotencyKey, LedgerTransactionType.Reservation, cancellationToken);
        if (existingTransaction is not null)
        {
            var existingReservation = await _repository.GetReservationByExternalIdAsync(existingTransaction.ExternalTransactionId, cancellationToken)
                ?? throw new LedgerConflictException("Idempotency key is already used by a reservation transaction without a reservation.");

            return Map(existingReservation, existingReservation.LedgerAccount);
        }

        var account = await GetRequiredAccountAsync(command.AccountId, cancellationToken);
        EnsureAccountCanPost(account);
        EnsureCurrency(account, currencyCode);
        var balance = await GetRequiredBalanceAsync(account, cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        ExecuteDomain(() => balance.Reserve(command.Amount, now));

        var reservation = CreateDomain(() => FundsReservation.Create(
            command.ExternalTransactionId,
            account.Id,
            command.Amount,
            currencyCode,
            now,
            command.ExpiresAtUtc));

        var ledgerTransaction = CreateTransaction(
            command.ExternalTransactionId,
            command.IdempotencyKey,
            LedgerTransactionType.Reservation,
            command.Amount,
            currencyCode,
            command.Description,
            now);
        AddSelfBalancingEntries(ledgerTransaction, account, command.Amount, now);
        ExecuteDomain(() => ledgerTransaction.Complete(now));

        await _repository.AddReservationAsync(reservation, cancellationToken);
        await _repository.AddLedgerTransactionAsync(ledgerTransaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await operation.CommitAsync(cancellationToken);

        return Map(reservation, account);
    }

    public async Task<FundsReservationDto> ReleaseReservationAsync(ReleaseReservationCommand command, CancellationToken cancellationToken = default)
    {
        await using var operation = await _repository.BeginTransactionAsync(cancellationToken);
        var existingTransaction = await GetIdempotentTransactionAsync(command.IdempotencyKey, LedgerTransactionType.ReservationRelease, cancellationToken);
        if (existingTransaction is not null)
        {
            var existingReservation = await GetRequiredReservationAsync(command.ReservationId, cancellationToken);
            return Map(existingReservation, existingReservation.LedgerAccount);
        }

        var reservation = await GetRequiredReservationAsync(command.ReservationId, cancellationToken);
        var account = reservation.LedgerAccount ?? throw new LedgerNotFoundException($"Ledger account '{reservation.LedgerAccountId}' was not found.");
        EnsureAccountCanPost(account);
        var balance = await GetRequiredBalanceAsync(account, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        ExecuteDomain(() =>
        {
            reservation.Release(now);
            balance.Release(reservation.Amount, now);
        });

        var ledgerTransaction = CreateTransaction(
            reservation.ExternalTransactionId,
            command.IdempotencyKey,
            LedgerTransactionType.ReservationRelease,
            reservation.Amount,
            reservation.CurrencyCode,
            command.Description,
            now);
        AddSelfBalancingEntries(ledgerTransaction, account, reservation.Amount, now);
        ExecuteDomain(() => ledgerTransaction.Complete(now));

        await _repository.AddLedgerTransactionAsync(ledgerTransaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await operation.CommitAsync(cancellationToken);

        return Map(reservation, account);
    }

    public async Task<LedgerTransactionDto> CaptureReservationAsync(CaptureReservationCommand command, CancellationToken cancellationToken = default)
    {
        await using var operation = await _repository.BeginTransactionAsync(cancellationToken);
        var existingTransaction = await GetIdempotentTransactionAsync(command.IdempotencyKey, LedgerTransactionType.ReservationCapture, cancellationToken);
        if (existingTransaction is not null)
        {
            return Map(existingTransaction);
        }

        var reservation = await GetRequiredReservationAsync(command.ReservationId, cancellationToken);
        var debitAccount = reservation.LedgerAccount ?? throw new LedgerNotFoundException($"Ledger account '{reservation.LedgerAccountId}' was not found.");
        var creditAccount = await GetRequiredAccountAsync(command.CreditAccountId, cancellationToken);
        EnsureAccountCanPost(debitAccount);
        EnsureAccountCanPost(creditAccount);
        EnsureCurrency(debitAccount, reservation.CurrencyCode);
        EnsureCurrency(creditAccount, reservation.CurrencyCode);

        var debitBalance = await GetRequiredBalanceAsync(debitAccount, cancellationToken);
        var creditBalance = await GetRequiredBalanceAsync(creditAccount, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        ExecuteDomain(() =>
        {
            reservation.Confirm(now);
            debitBalance.Capture(reservation.Amount, now);
            creditBalance.CreditAvailable(reservation.Amount, now);
        });

        var externalTransactionId = string.IsNullOrWhiteSpace(command.ExternalTransactionId)
            ? reservation.ExternalTransactionId
            : command.ExternalTransactionId;
        var ledgerTransaction = CreateTransaction(
            externalTransactionId!,
            command.IdempotencyKey,
            LedgerTransactionType.ReservationCapture,
            reservation.Amount,
            reservation.CurrencyCode,
            command.Description,
            now);
        AddTransferEntries(ledgerTransaction, debitAccount, creditAccount, reservation.Amount, now);
        ExecuteDomain(() => ledgerTransaction.Complete(now));

        await _repository.AddLedgerTransactionAsync(ledgerTransaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await operation.CommitAsync(cancellationToken);

        return Map(ledgerTransaction, AccountsById(debitAccount, creditAccount));
    }

    public async Task<LedgerTransactionDto> CreateTransferAsync(CreateTransferCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAmount(command.Amount);
        var currencyCode = NormalizeCurrency(command.CurrencyCode);
        await using var operation = await _repository.BeginTransactionAsync(cancellationToken);

        var existingTransaction = await GetIdempotentTransactionAsync(command.IdempotencyKey, LedgerTransactionType.Transfer, cancellationToken);
        if (existingTransaction is not null)
        {
            return Map(existingTransaction);
        }

        var debitAccount = await GetRequiredAccountAsync(command.DebitAccountId, cancellationToken);
        var creditAccount = await GetRequiredAccountAsync(command.CreditAccountId, cancellationToken);
        if (debitAccount.Id == creditAccount.Id)
        {
            throw new LedgerValidationException("Debit and credit accounts must be different.");
        }

        EnsureAccountCanPost(debitAccount);
        EnsureAccountCanPost(creditAccount);
        EnsureCurrency(debitAccount, currencyCode);
        EnsureCurrency(creditAccount, currencyCode);

        var debitBalance = await GetRequiredBalanceAsync(debitAccount, cancellationToken);
        var creditBalance = await GetRequiredBalanceAsync(creditAccount, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        ExecuteDomain(() =>
        {
            debitBalance.DebitAvailable(command.Amount, now);
            creditBalance.CreditAvailable(command.Amount, now);
        });

        var ledgerTransaction = CreateTransaction(
            command.ExternalTransactionId,
            command.IdempotencyKey,
            LedgerTransactionType.Transfer,
            command.Amount,
            currencyCode,
            command.Description,
            now);
        AddTransferEntries(ledgerTransaction, debitAccount, creditAccount, command.Amount, now);
        ExecuteDomain(() => ledgerTransaction.Complete(now));

        await _repository.AddLedgerTransactionAsync(ledgerTransaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await operation.CommitAsync(cancellationToken);

        return Map(ledgerTransaction, AccountsById(debitAccount, creditAccount));
    }

    public async Task<LedgerTransactionDto> ReverseLedgerTransactionAsync(ReverseLedgerTransactionCommand command, CancellationToken cancellationToken = default)
    {
        await using var operation = await _repository.BeginTransactionAsync(cancellationToken);
        var existingTransaction = await GetIdempotentTransactionAsync(command.IdempotencyKey, LedgerTransactionType.Reversal, cancellationToken);
        if (existingTransaction is not null)
        {
            return Map(existingTransaction);
        }

        var original = await _repository.GetTransactionByIdAsync(command.TransactionId, cancellationToken)
            ?? throw new LedgerNotFoundException($"Ledger transaction '{command.TransactionId}' was not found.");
        if (original.Status == LedgerTransactionStatus.Reversed || await _repository.GetReversalForTransactionAsync(original.Id, cancellationToken) is not null)
        {
            throw new LedgerConflictException("Ledger transaction cannot be reversed twice.");
        }

        if (original.Status != LedgerTransactionStatus.Completed)
        {
            throw new LedgerConflictException("Only completed ledger transactions can be reversed.");
        }

        var accounts = original.Entries
            .Select(x => x.LedgerAccount)
            .OfType<LedgerAccount>()
            .DistinctBy(x => x.Id)
            .ToDictionary(x => x.Id);
        var now = _dateTimeProvider.UtcNow;
        var externalTransactionId = string.IsNullOrWhiteSpace(command.ExternalTransactionId)
            ? $"reversal:{original.Id}"
            : command.ExternalTransactionId!;
        var reversal = CreateTransaction(
            externalTransactionId,
            command.IdempotencyKey,
            LedgerTransactionType.Reversal,
            original.Amount,
            original.CurrencyCode,
            command.Description,
            now,
            original.Id);

        foreach (var entry in original.Entries)
        {
            if (!accounts.TryGetValue(entry.LedgerAccountId, out var account))
            {
                throw new LedgerNotFoundException($"Ledger account '{entry.LedgerAccountId}' was not found.");
            }

            EnsureAccountCanPost(account);
            var balance = await GetRequiredBalanceAsync(account, cancellationToken);
            ExecuteDomain(() =>
            {
                if (entry.Direction == LedgerEntryDirection.Debit)
                {
                    balance.CreditAvailable(entry.Amount, now);
                    reversal.AddEntry(entry.LedgerAccountId, LedgerEntryDirection.Credit, entry.Amount, now);
                }
                else
                {
                    balance.DebitAvailable(entry.Amount, now);
                    reversal.AddEntry(entry.LedgerAccountId, LedgerEntryDirection.Debit, entry.Amount, now);
                }
            });
        }

        ExecuteDomain(() =>
        {
            reversal.Complete(now);
            original.MarkReversed(now);
        });

        await _repository.AddLedgerTransactionAsync(reversal, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await operation.CommitAsync(cancellationToken);

        return Map(reversal, accounts);
    }

    public async Task<LedgerTransactionDto> GetLedgerTransactionAsync(GetLedgerTransactionQuery query, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetTransactionByIdAsync(query.TransactionId, cancellationToken)
            ?? throw new LedgerNotFoundException($"Ledger transaction '{query.TransactionId}' was not found.");

        return Map(transaction);
    }

    public async Task<LedgerTransactionDto> GetLedgerTransactionByExternalIdAsync(GetLedgerTransactionByExternalIdQuery query, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetTransactionByExternalIdAsync(query.ExternalTransactionId, cancellationToken)
            ?? throw new LedgerNotFoundException($"Ledger transaction '{query.ExternalTransactionId}' was not found.");

        return Map(transaction);
    }

    public async Task<IReadOnlyCollection<LedgerEntryDto>> GetAccountEntriesAsync(GetAccountEntriesQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Skip < 0)
        {
            throw new LedgerValidationException("Skip cannot be negative.");
        }

        var take = Math.Clamp(query.Take <= 0 ? 100 : query.Take, 1, MaxEntriesTake);
        _ = await GetRequiredAccountAsync(query.AccountId, cancellationToken);
        var entries = await _repository.GetEntriesByAccountIdAsync(query.AccountId, query.Skip, take, cancellationToken);
        return entries.Select(entry => Map(entry)).ToArray();
    }

    private async Task<LedgerTransaction?> GetIdempotentTransactionAsync(string idempotencyKey, LedgerTransactionType expectedType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new LedgerValidationException("IdempotencyKey is required.");
        }

        var existing = await _repository.GetTransactionByIdempotencyKeyAsync(idempotencyKey.Trim(), cancellationToken);
        if (existing is not null && existing.Type != expectedType)
        {
            throw new LedgerConflictException("IdempotencyKey is already used by a different ledger operation.");
        }

        return existing;
    }

    private async Task<LedgerAccount> GetRequiredAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _repository.GetAccountByAccountIdAsync(accountId, cancellationToken)
            ?? throw new LedgerNotFoundException($"Ledger account for account '{accountId}' was not found.");
    }

    private async Task<AccountBalance> GetRequiredBalanceAsync(LedgerAccount account, CancellationToken cancellationToken)
    {
        return await _repository.GetBalanceByLedgerAccountIdAsync(account.Id, cancellationToken)
            ?? throw new LedgerNotFoundException($"Balance for ledger account '{account.Id}' was not found.");
    }

    private async Task<FundsReservation> GetRequiredReservationAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        return await _repository.GetReservationByIdAsync(reservationId, cancellationToken)
            ?? throw new LedgerNotFoundException($"Funds reservation '{reservationId}' was not found.");
    }

    private static void AddTransferEntries(LedgerTransaction transaction, LedgerAccount debitAccount, LedgerAccount creditAccount, decimal amount, DateTimeOffset createdAtUtc)
    {
        transaction.AddEntry(debitAccount.Id, LedgerEntryDirection.Debit, amount, createdAtUtc);
        transaction.AddEntry(creditAccount.Id, LedgerEntryDirection.Credit, amount, createdAtUtc);
    }

    private static void AddSelfBalancingEntries(LedgerTransaction transaction, LedgerAccount account, decimal amount, DateTimeOffset createdAtUtc)
    {
        transaction.AddEntry(account.Id, LedgerEntryDirection.Debit, amount, createdAtUtc);
        transaction.AddEntry(account.Id, LedgerEntryDirection.Credit, amount, createdAtUtc);
    }

    private static LedgerTransaction CreateTransaction(
        string externalTransactionId,
        string idempotencyKey,
        LedgerTransactionType type,
        decimal amount,
        string currencyCode,
        string? description,
        DateTimeOffset createdAtUtc,
        Guid? reversedTransactionId = null)
    {
        return CreateDomain(() => LedgerTransaction.Create(externalTransactionId, idempotencyKey, type, amount, currencyCode, description, createdAtUtc, reversedTransactionId));
    }

    private static void EnsureAccountCanPost(LedgerAccount account)
    {
        try
        {
            account.EnsureActive();
        }
        catch (InvalidOperationException exception)
        {
            throw new LedgerConflictException(exception.Message);
        }
    }

    private static void EnsureCurrency(LedgerAccount account, string currencyCode)
    {
        if (!string.Equals(account.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new LedgerValidationException("Currency must match between accounts.");
        }
    }

    private static void EnsureAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new LedgerValidationException("Amount must be greater than zero.");
        }
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
        {
            throw new LedgerValidationException("CurrencyCode must contain exactly three characters.");
        }

        return currencyCode.Trim().ToUpperInvariant();
    }

    private static void ExecuteDomain(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException exception)
        {
            throw new LedgerValidationException(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            throw new LedgerConflictException(exception.Message);
        }
    }

    private static T CreateDomain<T>(Func<T> factory)
    {
        try
        {
            return factory();
        }
        catch (ArgumentException exception)
        {
            throw new LedgerValidationException(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            throw new LedgerConflictException(exception.Message);
        }
    }

    private static IReadOnlyDictionary<Guid, LedgerAccount> AccountsById(params LedgerAccount[] accounts)
    {
        return accounts.DistinctBy(x => x.Id).ToDictionary(x => x.Id);
    }

    private static LedgerAccountDto Map(LedgerAccount account)
    {
        return new LedgerAccountDto(
            account.Id,
            account.AccountId,
            account.ParticipantId,
            account.AccountNumber,
            account.CurrencyCode,
            account.Status.ToString(),
            account.CreatedAtUtc);
    }

    private static AccountBalanceDto Map(AccountBalance balance, LedgerAccount? account)
    {
        return new AccountBalanceDto(
            balance.Id,
            balance.LedgerAccountId,
            account?.AccountId ?? Guid.Empty,
            balance.AvailableBalance,
            balance.ReservedBalance,
            balance.CurrencyCode,
            balance.UpdatedAtUtc);
    }

    private static FundsReservationDto Map(FundsReservation reservation, LedgerAccount? account)
    {
        return new FundsReservationDto(
            reservation.Id,
            reservation.ExternalTransactionId,
            reservation.LedgerAccountId,
            account?.AccountId ?? Guid.Empty,
            reservation.Amount,
            reservation.CurrencyCode,
            reservation.Status.ToString(),
            reservation.CreatedAtUtc,
            reservation.ConfirmedAtUtc,
            reservation.ReleasedAtUtc,
            reservation.ExpiresAtUtc);
    }

    private static LedgerTransactionDto Map(LedgerTransaction transaction, IReadOnlyDictionary<Guid, LedgerAccount>? accounts = null)
    {
        return new LedgerTransactionDto(
            transaction.Id,
            transaction.ExternalTransactionId,
            transaction.IdempotencyKey,
            transaction.Type.ToString(),
            transaction.Status.ToString(),
            transaction.Amount,
            transaction.CurrencyCode,
            transaction.Description,
            transaction.CreatedAtUtc,
            transaction.CompletedAtUtc,
            transaction.FailedAtUtc,
            transaction.ReversedTransactionId,
            transaction.Entries.OrderBy(x => x.CreatedAtUtc).Select(entry => Map(entry, accounts)).ToArray());
    }

    private static LedgerEntryDto Map(LedgerEntry entry, IReadOnlyDictionary<Guid, LedgerAccount>? accounts = null)
    {
        var account = entry.LedgerAccount;
        if (account is null && accounts is not null)
        {
            accounts.TryGetValue(entry.LedgerAccountId, out account);
        }

        return new LedgerEntryDto(
            entry.Id,
            entry.LedgerTransactionId,
            entry.LedgerAccountId,
            account?.AccountId ?? Guid.Empty,
            entry.Direction.ToString(),
            entry.Amount,
            entry.CurrencyCode,
            entry.CreatedAtUtc);
    }
}

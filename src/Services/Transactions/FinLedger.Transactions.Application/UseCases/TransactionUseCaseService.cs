using FinLedger.Contracts.Transactions;
using FinLedger.Transactions.Application.Abstractions;
using FinLedger.Transactions.Application.Exceptions;
using FinLedger.Transactions.Domain.Entities;
using FinLedger.Transactions.Domain.Enums;

namespace FinLedger.Transactions.Application.UseCases;

public sealed class TransactionUseCaseService : ITransactionUseCaseService
{
    private const int MaxTake = 500;

    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankIntegrationClient _bankIntegrationClient;
    private readonly ILedgerClient _ledgerClient;
    private readonly IAuditClient _auditClient;
    private readonly ITransactionNumberGenerator _transactionNumberGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TransactionUseCaseService(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork,
        IBankIntegrationClient bankIntegrationClient,
        ILedgerClient ledgerClient,
        IAuditClient auditClient,
        ITransactionNumberGenerator transactionNumberGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _bankIntegrationClient = bankIntegrationClient;
        _ledgerClient = ledgerClient;
        _auditClient = auditClient;
        _transactionNumberGenerator = transactionNumberGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionCommand command, CancellationToken cancellationToken = default)
    {
        ValidateCreate(command);
        var now = _dateTimeProvider.UtcNow;
        var transaction = CreateDomain(() => Transaction.Create(
            _transactionNumberGenerator.Generate(),
            command.PayerParticipantId,
            command.ReceiverParticipantId,
            command.PayerAccountId,
            command.ReceiverAccountId,
            command.PayerBankCode,
            command.PayerAccountNumber,
            command.ReceiverBankCode,
            command.ReceiverAccountNumber,
            command.Amount,
            command.CurrencyCode,
            command.Description,
            command.ExternalReference,
            now));

        await _repository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await AuditSafeAsync(transaction, "CreateTransaction", "Created", "Transaction created.", cancellationToken);

        return Map(transaction);
    }

    public async Task<TransactionDto> ProcessTransactionAsync(ProcessTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var transaction = await GetRequiredAsync(command.TransactionId, cancellationToken);
        if (transaction.Status is TransactionStatus.Completed or TransactionStatus.Cancelled or TransactionStatus.Reversed)
        {
            throw new TransactionConflictException($"Transaction '{transaction.TransactionNumber}' is already terminal.");
        }

        await AuditSafeAsync(transaction, "ProcessTransaction", "Started", "Transaction processing started.", cancellationToken);

        ExecuteDomain(() => transaction.MarkPayerValidationPending(_dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var payerValidation = await _bankIntegrationClient.ValidatePayerAsync(transaction, cancellationToken);
        if (!payerValidation.Succeeded)
        {
            ExecuteDomain(() => transaction.MarkPayerValidationFailed(payerValidation.Message, _dateTimeProvider.UtcNow));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditSafeAsync(transaction, "ProcessTransaction", "Failed", payerValidation.Message, cancellationToken);
            return Map(transaction);
        }

        ExecuteDomain(() => transaction.MarkPayerValidated(_dateTimeProvider.UtcNow));
        ExecuteDomain(() => transaction.MarkReceiverValidationPending(_dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var receiverValidation = await _bankIntegrationClient.ValidateReceiverAsync(transaction, cancellationToken);
        if (!receiverValidation.Succeeded)
        {
            ExecuteDomain(() => transaction.MarkReceiverValidationFailed(receiverValidation.Message, _dateTimeProvider.UtcNow));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditSafeAsync(transaction, "ProcessTransaction", "Failed", receiverValidation.Message, cancellationToken);
            return Map(transaction);
        }

        ExecuteDomain(() => transaction.MarkReceiverValidated(_dateTimeProvider.UtcNow));
        ExecuteDomain(() => transaction.MarkFundsReservationPending(_dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reservation = await _ledgerClient.ReserveFundsAsync(transaction, cancellationToken);
        if (!reservation.Succeeded || reservation.ReservationId is null)
        {
            var reason = string.IsNullOrWhiteSpace(reservation.Message) ? "Funds reservation failed." : reservation.Message;
            ExecuteDomain(() => transaction.MarkFundsReservationFailed(reason, _dateTimeProvider.UtcNow));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditSafeAsync(transaction, "ProcessTransaction", "Failed", reason, cancellationToken);
            return Map(transaction);
        }

        ExecuteDomain(() => transaction.MarkFundsReserved(reservation.ReservationId.Value, _dateTimeProvider.UtcNow));
        ExecuteDomain(() => transaction.MarkPostingPending(_dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var commit = await _ledgerClient.CommitTransferAsync(transaction, reservation.ReservationId.Value, cancellationToken);
        if (!commit.Succeeded || commit.LedgerTransactionId is null)
        {
            var reason = string.IsNullOrWhiteSpace(commit.Message) ? "Ledger posting failed." : commit.Message;
            var reverse = await _ledgerClient.ReverseAsync(transaction, reservation.ReservationId.Value, commit.LedgerTransactionId, reason, cancellationToken);
            if (reverse.Succeeded)
            {
                ExecuteDomain(() => transaction.MarkReversed(reason, _dateTimeProvider.UtcNow));
                await AuditSafeAsync(transaction, "ProcessTransaction", "Reversed", reason, cancellationToken);
            }
            else
            {
                ExecuteDomain(() => transaction.MarkFailed($"{reason}; reversal failed: {reverse.Message}", _dateTimeProvider.UtcNow));
                await AuditSafeAsync(transaction, "ProcessTransaction", "Failed", transaction.FailureReason ?? reason, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(transaction);
        }

        ExecuteDomain(() => transaction.MarkCompleted(commit.LedgerTransactionId.Value, _dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await AuditSafeAsync(transaction, "ProcessTransaction", "Completed", "Transaction completed.", cancellationToken);

        return Map(transaction);
    }

    public async Task<TransactionDto> CancelTransactionAsync(CancelTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var transaction = await GetRequiredAsync(command.TransactionId, cancellationToken);
        ExecuteDomain(() => transaction.MarkCancelled(command.Reason, _dateTimeProvider.UtcNow));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await AuditSafeAsync(transaction, "CancelTransaction", "Cancelled", command.Reason ?? "Transaction cancelled.", cancellationToken);
        return Map(transaction);
    }

    public async Task<TransactionDto> GetTransactionByIdAsync(GetTransactionByIdQuery query, CancellationToken cancellationToken = default)
    {
        return Map(await GetRequiredAsync(query.TransactionId, cancellationToken));
    }

    public async Task<TransactionDto> GetTransactionByNumberAsync(GetTransactionByNumberQuery query, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByNumberAsync(query.TransactionNumber, cancellationToken)
            ?? throw new TransactionNotFoundException($"Transaction '{query.TransactionNumber}' was not found.");

        return Map(transaction);
    }

    public async Task<IReadOnlyCollection<TransactionDto>> GetTransactionsAsync(GetTransactionsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.Skip < 0)
        {
            throw new TransactionsValidationException("Skip cannot be negative.");
        }

        var take = Math.Clamp(query.Take <= 0 ? 100 : query.Take, 1, MaxTake);
        var status = ParseStatus(query.Status);
        var transactions = await _repository.GetAsync(status, query.ParticipantId, query.Skip, take, cancellationToken);
        return transactions.Select(Map).ToArray();
    }

    private async Task<Transaction> GetRequiredAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(transactionId, cancellationToken)
            ?? throw new TransactionNotFoundException($"Transaction '{transactionId}' was not found.");
    }

    private async Task AuditSafeAsync(Transaction transaction, string operation, string outcome, string details, CancellationToken cancellationToken)
    {
        try
        {
            await _auditClient.RecordTransactionEventAsync(transaction, operation, outcome, details, cancellationToken);
        }
        catch
        {
            // Audit is required logically, but a transient audit outage should not hide the transaction state transition.
        }
    }

    private static void ValidateCreate(CreateTransactionCommand command)
    {
        if (command.Amount <= 0)
        {
            throw new TransactionsValidationException("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(command.CurrencyCode) || command.CurrencyCode.Trim().Length != 3)
        {
            throw new TransactionsValidationException("CurrencyCode is required and must contain exactly three characters.");
        }

        if (command.PayerAccountId == command.ReceiverAccountId)
        {
            throw new TransactionsValidationException("PayerAccountId and ReceiverAccountId must be different.");
        }

        if (command.PayerParticipantId == Guid.Empty)
        {
            throw new TransactionsValidationException("PayerParticipantId is required.");
        }

        if (command.ReceiverParticipantId == Guid.Empty)
        {
            throw new TransactionsValidationException("ReceiverParticipantId is required.");
        }
    }

    private static TransactionStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (Enum.TryParse<TransactionStatus>(status, ignoreCase: true, out var parsed) && Enum.IsDefined(typeof(TransactionStatus), parsed))
        {
            return parsed;
        }

        throw new TransactionsValidationException($"'{status}' is not a valid transaction status.");
    }

    private static void ExecuteDomain(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException exception)
        {
            throw new TransactionsValidationException(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            throw new TransactionConflictException(exception.Message);
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
            throw new TransactionsValidationException(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            throw new TransactionConflictException(exception.Message);
        }
    }

    private static TransactionDto Map(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.TransactionNumber,
            transaction.PayerParticipantId,
            transaction.ReceiverParticipantId,
            transaction.PayerAccountId,
            transaction.ReceiverAccountId,
            transaction.PayerBankCode,
            transaction.PayerAccountNumber,
            transaction.ReceiverBankCode,
            transaction.ReceiverAccountNumber,
            transaction.Amount,
            transaction.CurrencyCode,
            transaction.Description,
            transaction.ExternalReference,
            transaction.Status.ToString(),
            transaction.LedgerReservationId,
            transaction.LedgerTransactionId,
            transaction.FailureReason,
            transaction.CreatedAtUtc,
            transaction.UpdatedAtUtc,
            transaction.CompletedAtUtc,
            transaction.CancelledAtUtc,
            transaction.History.OrderBy(x => x.CreatedAtUtc).Select(Map).ToArray());
    }

    private static TransactionHistoryDto Map(TransactionHistory history)
    {
        return new TransactionHistoryDto(history.Id, history.TransactionId, history.Status.ToString(), history.Reason, history.CreatedAtUtc);
    }
}

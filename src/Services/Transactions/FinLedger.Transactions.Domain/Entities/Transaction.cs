using FinLedger.Transactions.Domain.Enums;

namespace FinLedger.Transactions.Domain.Entities;

public sealed class Transaction
{
    private readonly List<TransactionHistory> _history = [];

    private Transaction()
    {
    }

    private Transaction(
        string transactionNumber,
        Guid payerParticipantId,
        Guid receiverParticipantId,
        Guid payerAccountId,
        Guid receiverAccountId,
        string payerBankCode,
        string payerAccountNumber,
        string receiverBankCode,
        string receiverAccountNumber,
        decimal amount,
        string currencyCode,
        string? description,
        string? externalReference,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        TransactionNumber = transactionNumber;
        PayerParticipantId = payerParticipantId;
        ReceiverParticipantId = receiverParticipantId;
        PayerAccountId = payerAccountId;
        ReceiverAccountId = receiverAccountId;
        PayerBankCode = payerBankCode;
        PayerAccountNumber = payerAccountNumber;
        ReceiverBankCode = receiverBankCode;
        ReceiverAccountNumber = receiverAccountNumber;
        Amount = amount;
        CurrencyCode = NormalizeCurrency(currencyCode);
        Description = description;
        ExternalReference = externalReference;
        CreatedAtUtc = createdAtUtc;
        Status = TransactionStatus.Created;
        AddHistory(Status, "Transaction created", createdAtUtc);
    }

    public Guid Id { get; private set; }

    public string TransactionNumber { get; private set; } = string.Empty;

    public Guid PayerParticipantId { get; private set; }

    public Guid ReceiverParticipantId { get; private set; }

    public Guid PayerAccountId { get; private set; }

    public Guid ReceiverAccountId { get; private set; }

    public string PayerBankCode { get; private set; } = string.Empty;

    public string PayerAccountNumber { get; private set; } = string.Empty;

    public string ReceiverBankCode { get; private set; } = string.Empty;

    public string ReceiverAccountNumber { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ExternalReference { get; private set; }

    public TransactionStatus Status { get; private set; }

    public Guid? LedgerReservationId { get; private set; }

    public Guid? LedgerTransactionId { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<TransactionHistory> History => _history;

    public static Transaction Create(
        string transactionNumber,
        Guid payerParticipantId,
        Guid receiverParticipantId,
        Guid payerAccountId,
        Guid receiverAccountId,
        string payerBankCode,
        string payerAccountNumber,
        string receiverBankCode,
        string receiverAccountNumber,
        decimal amount,
        string currencyCode,
        string? description,
        string? externalReference,
        DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(transactionNumber))
        {
            throw new ArgumentException("TransactionNumber is required.", nameof(transactionNumber));
        }

        if (payerParticipantId == Guid.Empty)
        {
            throw new ArgumentException("PayerParticipantId is required.", nameof(payerParticipantId));
        }

        if (receiverParticipantId == Guid.Empty)
        {
            throw new ArgumentException("ReceiverParticipantId is required.", nameof(receiverParticipantId));
        }

        if (payerAccountId == Guid.Empty)
        {
            throw new ArgumentException("PayerAccountId is required.", nameof(payerAccountId));
        }

        if (receiverAccountId == Guid.Empty)
        {
            throw new ArgumentException("ReceiverAccountId is required.", nameof(receiverAccountId));
        }

        if (payerAccountId == receiverAccountId)
        {
            throw new InvalidOperationException("PayerAccountId and ReceiverAccountId must be different.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
        {
            throw new ArgumentException("CurrencyCode must contain exactly three characters.", nameof(currencyCode));
        }

        return new Transaction(
            transactionNumber.Trim(),
            payerParticipantId,
            receiverParticipantId,
            payerAccountId,
            receiverAccountId,
            Required(payerBankCode, nameof(payerBankCode)),
            Required(payerAccountNumber, nameof(payerAccountNumber)),
            Required(receiverBankCode, nameof(receiverBankCode)),
            Required(receiverAccountNumber, nameof(receiverAccountNumber)),
            amount,
            currencyCode,
            description,
            externalReference,
            createdAtUtc);
    }

    public void MarkPayerValidationPending(DateTimeOffset changedAtUtc) => ChangeStatus(TransactionStatus.PayerValidationPending, null, changedAtUtc);

    public void MarkPayerValidated(DateTimeOffset changedAtUtc) => ChangeStatus(TransactionStatus.PayerValidated, null, changedAtUtc);

    public void MarkPayerValidationFailed(string reason, DateTimeOffset changedAtUtc)
    {
        ChangeStatus(TransactionStatus.PayerValidationFailed, reason, changedAtUtc);
        MarkFailed(reason, changedAtUtc);
    }

    public void MarkReceiverValidationPending(DateTimeOffset changedAtUtc) => ChangeStatus(TransactionStatus.ReceiverValidationPending, null, changedAtUtc);

    public void MarkReceiverValidated(DateTimeOffset changedAtUtc) => ChangeStatus(TransactionStatus.ReceiverValidated, null, changedAtUtc);

    public void MarkReceiverValidationFailed(string reason, DateTimeOffset changedAtUtc)
    {
        ChangeStatus(TransactionStatus.ReceiverValidationFailed, reason, changedAtUtc);
        MarkFailed(reason, changedAtUtc);
    }

    public void MarkFundsReservationPending(DateTimeOffset changedAtUtc) => ChangeStatus(TransactionStatus.FundsReservationPending, null, changedAtUtc);

    public void MarkFundsReserved(Guid ledgerReservationId, DateTimeOffset changedAtUtc)
    {
        LedgerReservationId = ledgerReservationId;
        ChangeStatus(TransactionStatus.FundsReserved, null, changedAtUtc);
    }

    public void MarkFundsReservationFailed(string reason, DateTimeOffset changedAtUtc)
    {
        ChangeStatus(TransactionStatus.FundsReservationFailed, reason, changedAtUtc);
        MarkFailed(reason, changedAtUtc);
    }

    public void MarkPostingPending(DateTimeOffset changedAtUtc) => ChangeStatus(TransactionStatus.PostingPending, null, changedAtUtc);

    public void MarkCompleted(Guid ledgerTransactionId, DateTimeOffset changedAtUtc)
    {
        LedgerTransactionId = ledgerTransactionId;
        CompletedAtUtc = changedAtUtc;
        ChangeStatus(TransactionStatus.Completed, null, changedAtUtc);
    }

    public void MarkFailed(string reason, DateTimeOffset changedAtUtc)
    {
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "Transaction failed." : reason;
        CompletedAtUtc = changedAtUtc;
        ChangeStatus(TransactionStatus.Failed, FailureReason, changedAtUtc);
    }

    public void MarkCancelled(string? reason, DateTimeOffset changedAtUtc)
    {
        if (Status is TransactionStatus.Completed or TransactionStatus.Reversed)
        {
            throw new InvalidOperationException("Completed or reversed transactions cannot be cancelled.");
        }

        CancelledAtUtc = changedAtUtc;
        CompletedAtUtc = changedAtUtc;
        ChangeStatus(TransactionStatus.Cancelled, string.IsNullOrWhiteSpace(reason) ? "Transaction cancelled" : reason, changedAtUtc);
    }

    public void MarkReversed(string reason, DateTimeOffset changedAtUtc)
    {
        CompletedAtUtc = changedAtUtc;
        FailureReason = reason;
        ChangeStatus(TransactionStatus.Reversed, reason, changedAtUtc);
    }

    private void ChangeStatus(TransactionStatus status, string? reason, DateTimeOffset changedAtUtc)
    {
        if (Status == TransactionStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled transactions cannot change status.");
        }

        if (Status == TransactionStatus.Completed && status != TransactionStatus.Reversed)
        {
            throw new InvalidOperationException("Completed transactions cannot change status.");
        }

        if (Status == status)
        {
            return;
        }

        Status = status;
        UpdatedAtUtc = changedAtUtc;
        AddHistory(status, reason, changedAtUtc);
    }

    private void AddHistory(TransactionStatus status, string? reason, DateTimeOffset createdAtUtc)
    {
        _history.Add(TransactionHistory.Create(Id, status, reason, createdAtUtc));
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}

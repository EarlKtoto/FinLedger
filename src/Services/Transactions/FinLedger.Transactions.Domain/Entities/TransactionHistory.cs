using FinLedger.Transactions.Domain.Enums;

namespace FinLedger.Transactions.Domain.Entities;

public sealed class TransactionHistory
{
    private TransactionHistory()
    {
    }

    private TransactionHistory(Guid transactionId, TransactionStatus status, string? reason, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        Status = status;
        Reason = reason;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid TransactionId { get; private set; }

    public Transaction? Transaction { get; private set; }

    public TransactionStatus Status { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static TransactionHistory Create(Guid transactionId, TransactionStatus status, string? reason, DateTimeOffset createdAtUtc)
    {
        return new TransactionHistory(transactionId, status, reason, createdAtUtc);
    }
}

namespace FinLedger.Transactions.Domain.Enums;

public enum TransactionStatus
{
    Created = 1,
    PayerValidationPending = 2,
    PayerValidated = 3,
    PayerValidationFailed = 4,
    ReceiverValidationPending = 5,
    ReceiverValidated = 6,
    ReceiverValidationFailed = 7,
    FundsReservationPending = 8,
    FundsReserved = 9,
    FundsReservationFailed = 10,
    PostingPending = 11,
    Completed = 12,
    Failed = 13,
    Cancelled = 14,
    Reversed = 15
}

namespace FinLedger.Contracts.Transactions;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}

public sealed record PaymentRequestDto(string PayerBankCode, string PayerAccountNumber, string RecipientBankCode, string RecipientAccountNumber, decimal Amount, string Currency, string ExternalReference);

public sealed record PaymentResultDto(Guid TransactionId, PaymentStatus Status, string Message);
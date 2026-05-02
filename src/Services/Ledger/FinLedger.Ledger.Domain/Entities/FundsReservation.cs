using FinLedger.Ledger.Domain.Enums;

namespace FinLedger.Ledger.Domain.Entities;

public sealed class FundsReservation
{
    private FundsReservation()
    {
    }

    private FundsReservation(string externalTransactionId, Guid ledgerAccountId, decimal amount, string currencyCode, DateTimeOffset createdAtUtc, DateTimeOffset? expiresAtUtc)
    {
        Id = Guid.NewGuid();
        ExternalTransactionId = externalTransactionId;
        LedgerAccountId = ledgerAccountId;
        Amount = amount;
        CurrencyCode = NormalizeCurrency(currencyCode);
        Status = FundsReservationStatus.Active;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public string ExternalTransactionId { get; private set; } = string.Empty;

    public Guid LedgerAccountId { get; private set; }

    public LedgerAccount? LedgerAccount { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public FundsReservationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ConfirmedAtUtc { get; private set; }

    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public static FundsReservation Create(string externalTransactionId, Guid ledgerAccountId, decimal amount, string currencyCode, DateTimeOffset createdAtUtc, DateTimeOffset? expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
        {
            throw new ArgumentException("ExternalTransactionId is required.", nameof(externalTransactionId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        return new FundsReservation(externalTransactionId.Trim(), ledgerAccountId, amount, currencyCode, createdAtUtc, expiresAtUtc);
    }

    public void Confirm(DateTimeOffset confirmedAtUtc)
    {
        if (Status == FundsReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Reservation cannot be captured twice.");
        }

        if (Status == FundsReservationStatus.Released)
        {
            throw new InvalidOperationException("Released reservation cannot be captured.");
        }

        if (Status == FundsReservationStatus.Expired)
        {
            throw new InvalidOperationException("Expired reservation cannot be captured.");
        }

        Status = FundsReservationStatus.Confirmed;
        ConfirmedAtUtc = confirmedAtUtc;
    }

    public void Release(DateTimeOffset releasedAtUtc)
    {
        if (Status == FundsReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Confirmed reservation cannot be released.");
        }

        if (Status == FundsReservationStatus.Released)
        {
            throw new InvalidOperationException("Reservation is already released.");
        }

        if (Status == FundsReservationStatus.Expired)
        {
            throw new InvalidOperationException("Expired reservation cannot be released.");
        }

        Status = FundsReservationStatus.Released;
        ReleasedAtUtc = releasedAtUtc;
    }

    private static string NormalizeCurrency(string currencyCode)
    {
        return currencyCode.Trim().ToUpperInvariant();
    }
}

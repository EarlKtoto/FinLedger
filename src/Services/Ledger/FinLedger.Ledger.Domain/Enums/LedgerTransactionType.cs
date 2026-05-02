namespace FinLedger.Ledger.Domain.Enums;

public enum LedgerTransactionType
{
    InitialDeposit = 1,
    Transfer = 2,
    Reservation = 3,
    ReservationRelease = 4,
    ReservationCapture = 5,
    Reversal = 6,
    Adjustment = 7,
    Fee = 8
}

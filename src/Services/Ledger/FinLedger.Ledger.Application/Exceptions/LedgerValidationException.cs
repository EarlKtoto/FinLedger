namespace FinLedger.Ledger.Application.Exceptions;

public sealed class LedgerValidationException : LedgerApplicationException
{
    public LedgerValidationException(string message) : base(message)
    {
    }
}

namespace FinLedger.Ledger.Application.Exceptions;

public sealed class LedgerConflictException : LedgerApplicationException
{
    public LedgerConflictException(string message) : base(message)
    {
    }
}

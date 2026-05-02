namespace FinLedger.Ledger.Application.Exceptions;

public sealed class LedgerNotFoundException : LedgerApplicationException
{
    public LedgerNotFoundException(string message) : base(message)
    {
    }
}

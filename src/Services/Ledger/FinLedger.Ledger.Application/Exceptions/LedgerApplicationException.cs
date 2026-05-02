namespace FinLedger.Ledger.Application.Exceptions;

public abstract class LedgerApplicationException : Exception
{
    protected LedgerApplicationException(string message) : base(message)
    {
    }
}

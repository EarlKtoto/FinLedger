namespace FinLedger.Accounts.Application.Exceptions;

public abstract class AccountsApplicationException : Exception
{
    protected AccountsApplicationException(string message)
        : base(message)
    {
    }
}

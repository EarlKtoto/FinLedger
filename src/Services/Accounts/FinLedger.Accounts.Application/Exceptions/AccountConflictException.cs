namespace FinLedger.Accounts.Application.Exceptions;

public sealed class AccountConflictException : AccountsApplicationException
{
    public AccountConflictException(string message)
        : base(message)
    {
    }
}

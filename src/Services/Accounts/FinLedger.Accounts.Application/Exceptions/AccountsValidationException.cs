namespace FinLedger.Accounts.Application.Exceptions;

public sealed class AccountsValidationException : AccountsApplicationException
{
    public AccountsValidationException(string message)
        : base(message)
    {
    }
}

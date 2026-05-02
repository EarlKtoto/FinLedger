namespace FinLedger.Accounts.Application.Exceptions;

public sealed class AccountNotFoundException : AccountsApplicationException
{
    public AccountNotFoundException(string message)
        : base(message)
    {
    }
}

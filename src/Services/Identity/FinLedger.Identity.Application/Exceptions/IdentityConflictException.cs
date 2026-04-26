namespace FinLedger.Identity.Application.Exceptions;

public sealed class IdentityConflictException : IdentityApplicationException
{
    public IdentityConflictException(string message)
        : base(message)
    {
    }
}

namespace FinLedger.Identity.Application.Exceptions;

public abstract class IdentityApplicationException : Exception
{
    protected IdentityApplicationException(string message)
        : base(message)
    {
    }
}

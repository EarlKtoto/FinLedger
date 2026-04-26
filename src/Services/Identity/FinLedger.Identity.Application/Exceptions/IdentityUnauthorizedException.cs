namespace FinLedger.Identity.Application.Exceptions;

public sealed class IdentityUnauthorizedException : IdentityApplicationException
{
    public IdentityUnauthorizedException(string message = "Authentication failed.")
        : base(message)
    {
    }
}

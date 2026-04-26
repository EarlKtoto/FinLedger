namespace FinLedger.Identity.Application.Exceptions;

public sealed class IdentityForbiddenException : IdentityApplicationException
{
    public IdentityForbiddenException(string message = "Authorization failed.")
        : base(message)
    {
    }
}

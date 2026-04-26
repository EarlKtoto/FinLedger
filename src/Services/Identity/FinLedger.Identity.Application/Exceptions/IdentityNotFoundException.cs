namespace FinLedger.Identity.Application.Exceptions;

public sealed class IdentityNotFoundException : IdentityApplicationException
{
    public IdentityNotFoundException(string message)
        : base(message)
    {
    }
}

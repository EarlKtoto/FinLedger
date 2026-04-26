namespace FinLedger.Identity.Application.Exceptions;

public sealed class IdentityValidationException : IdentityApplicationException
{
    public IdentityValidationException(string message)
        : base(message)
    {
    }

    public IdentityValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
    }
}

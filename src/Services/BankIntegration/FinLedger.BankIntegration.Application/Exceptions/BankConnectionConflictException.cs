namespace FinLedger.BankIntegration.Application.Exceptions;

public sealed class BankConnectionConflictException : BankIntegrationApplicationException
{
    public BankConnectionConflictException(string message) : base(message)
    {
    }
}

namespace FinLedger.BankIntegration.Application.Exceptions;

public sealed class BankIntegrationValidationException : BankIntegrationApplicationException
{
    public BankIntegrationValidationException(string message) : base(message)
    {
    }
}

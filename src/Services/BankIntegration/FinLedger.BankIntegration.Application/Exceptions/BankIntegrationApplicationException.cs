namespace FinLedger.BankIntegration.Application.Exceptions;

public abstract class BankIntegrationApplicationException : Exception
{
    protected BankIntegrationApplicationException(string message) : base(message)
    {
    }
}

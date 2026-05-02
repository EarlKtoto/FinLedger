namespace FinLedger.BankIntegration.Application.Exceptions;

public sealed class BankConnectionNotFoundException : BankIntegrationApplicationException
{
    public BankConnectionNotFoundException(string message) : base(message)
    {
    }
}

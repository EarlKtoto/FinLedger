namespace FinLedger.Transactions.Application.Exceptions;

public sealed class TransactionsValidationException : TransactionsApplicationException
{
    public TransactionsValidationException(string message) : base(message)
    {
    }
}

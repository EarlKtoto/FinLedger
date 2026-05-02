namespace FinLedger.Transactions.Application.Exceptions;

public abstract class TransactionsApplicationException : Exception
{
    protected TransactionsApplicationException(string message) : base(message)
    {
    }
}

namespace FinLedger.Transactions.Application.Exceptions;

public sealed class TransactionConflictException : TransactionsApplicationException
{
    public TransactionConflictException(string message) : base(message)
    {
    }
}

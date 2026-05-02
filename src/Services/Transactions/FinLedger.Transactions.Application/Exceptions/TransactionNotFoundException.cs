namespace FinLedger.Transactions.Application.Exceptions;

public sealed class TransactionNotFoundException : TransactionsApplicationException
{
    public TransactionNotFoundException(string message) : base(message)
    {
    }
}

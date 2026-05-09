using FinLedger.Gateway.Api.Contracts.Requests;
using FinLedger.Gateway.Api.Contracts.Responses;

namespace FinLedger.Gateway.Api.Clients;

public interface ITransactionsClient
{
    Task<CreateTransactionClientResponse> CreateTransactionAsync(CreateTransactionClientRequest request, CancellationToken cancellationToken = default);

    Task<ProcessTransactionClientResponse> ProcessTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<TransactionDetailsClientResponse> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<TransactionDetailsClientResponse> GetTransactionByNumberAsync(string transactionNumber, CancellationToken cancellationToken = default);

    Task<TransactionDetailsClientResponse> CancelTransactionAsync(Guid transactionId, CancelTransactionClientRequest request, CancellationToken cancellationToken = default);
}

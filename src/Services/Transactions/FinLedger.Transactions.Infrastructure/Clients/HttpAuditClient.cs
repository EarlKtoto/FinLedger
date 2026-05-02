using System.Net.Http.Json;
using FinLedger.Contracts.Audit;
using FinLedger.Transactions.Application.Abstractions;
using FinLedger.Transactions.Domain.Entities;

namespace FinLedger.Transactions.Infrastructure.Clients;

public sealed class HttpAuditClient : IAuditClient
{
    private readonly HttpClient _httpClient;

    public HttpAuditClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task RecordTransactionEventAsync(Transaction transaction, string operation, string outcome, string details, CancellationToken cancellationToken = default)
    {
        var request = new CreateAuditRecordRequest(
            "Transactions",
            operation,
            outcome,
            $"TransactionNumber={transaction.TransactionNumber}; Status={transaction.Status}; Details={details}");

        var response = await _httpClient.PostAsJsonAsync("/api/audit/events", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

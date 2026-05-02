using System.Net.Http.Json;
using FinLedger.Contracts.BankIntegration;
using FinLedger.Transactions.Application.Abstractions;
using FinLedger.Transactions.Domain.Entities;

namespace FinLedger.Transactions.Infrastructure.Clients;

public sealed class HttpBankIntegrationClient : IBankIntegrationClient
{
    private readonly HttpClient _httpClient;

    public HttpBankIntegrationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<ExternalOperationResult> ValidatePayerAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var request = new ValidatePayerBankRequest(
            transaction.PayerParticipantId,
            transaction.PayerAccountId,
            transaction.PayerBankCode,
            transaction.PayerAccountNumber,
            transaction.Amount,
            transaction.CurrencyCode,
            transaction.ExternalReference);

        return ValidateAsync("/api/bank-validation/payer", request, "Payer", cancellationToken);
    }

    public Task<ExternalOperationResult> ValidateReceiverAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var request = new ValidateReceiverBankRequest(
            transaction.ReceiverParticipantId,
            transaction.ReceiverAccountId,
            transaction.ReceiverBankCode,
            transaction.ReceiverAccountNumber,
            transaction.Amount,
            transaction.CurrencyCode,
            transaction.ExternalReference);

        return ValidateAsync("/api/bank-validation/receiver", request, "Receiver", cancellationToken);
    }

    private async Task<ExternalOperationResult> ValidateAsync<TRequest>(string path, TRequest request, string side, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ExternalOperationResult.Failure($"{side} validation failed with HTTP {(int)response.StatusCode}.");
            }

            var result = await response.Content.ReadFromJsonAsync<BankValidationResponse>(cancellationToken);
            if (result is null)
            {
                return ExternalOperationResult.Failure($"{side} validation returned an empty response.");
            }

            return result.IsValid
                ? ExternalOperationResult.Success(result.Message)
                : ExternalOperationResult.Failure($"{result.ErrorCode}: {result.Message}");
        }
        catch (HttpRequestException exception)
        {
            return ExternalOperationResult.Failure($"{side} validation failed: {exception.Message}");
        }
    }
}

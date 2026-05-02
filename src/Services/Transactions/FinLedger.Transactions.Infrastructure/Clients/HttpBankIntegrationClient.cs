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
        return ValidateAsync(new BankVerificationRequestDto(transaction.PayerBankCode, transaction.PayerAccountNumber), "Payer", cancellationToken);
    }

    public Task<ExternalOperationResult> ValidateReceiverAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        return ValidateAsync(new BankVerificationRequestDto(transaction.ReceiverBankCode, transaction.ReceiverAccountNumber), "Receiver", cancellationToken);
    }

    private async Task<ExternalOperationResult> ValidateAsync(BankVerificationRequestDto request, string side, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/banks/verify", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return ExternalOperationResult.Failure($"{side} validation failed with HTTP {(int)response.StatusCode}.");
            }

            var result = await response.Content.ReadFromJsonAsync<BankVerificationResultDto>(cancellationToken);
            if (result is null)
            {
                return ExternalOperationResult.Failure($"{side} validation returned an empty response.");
            }

            return result.IsAvailable
                ? ExternalOperationResult.Success(result.Message)
                : ExternalOperationResult.Failure(result.Message);
        }
        catch (HttpRequestException exception)
        {
            return ExternalOperationResult.Failure($"{side} validation failed: {exception.Message}");
        }
    }
}

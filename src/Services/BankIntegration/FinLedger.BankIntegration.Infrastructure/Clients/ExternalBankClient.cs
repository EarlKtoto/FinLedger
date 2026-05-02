using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinLedger.BankIntegration.Application.Abstractions;
using FinLedger.BankIntegration.Application.Constants;
using FinLedger.BankIntegration.Domain.Entities;
using FinLedger.Contracts.BankIntegration;

namespace FinLedger.BankIntegration.Infrastructure.Clients;

public sealed class ExternalBankClient : IExternalBankClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public ExternalBankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalBankCallResult> ValidateAsync(BankConnection connection, ExternalBankValidationRequest request, CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{connection.BaseUrl}/api/external-bank/validate";
        var requestPayload = JsonSerializer.Serialize(request, JsonOptions);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            httpRequest.Headers.TryAddWithoutValidation("X-API-KEY", connection.ApiKey);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responsePayload = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return Failure(BankIntegrationErrorCodes.BankUnauthorized, "External bank rejected API key.", requestUrl, requestPayload, responsePayload, response, stopwatch.ElapsedMilliseconds);
            }

            if ((int)response.StatusCode >= 500)
            {
                return Failure(BankIntegrationErrorCodes.BankUnavailable, "External bank is unavailable.", requestUrl, requestPayload, responsePayload, response, stopwatch.ElapsedMilliseconds);
            }

            if (!response.IsSuccessStatusCode)
            {
                return Failure(BankIntegrationErrorCodes.BankBadResponse, $"External bank returned HTTP {(int)response.StatusCode}.", requestUrl, requestPayload, responsePayload, response, stopwatch.ElapsedMilliseconds);
            }

            ExternalBankValidationResponse? bankResponse;
            try
            {
                bankResponse = JsonSerializer.Deserialize<ExternalBankValidationResponse>(responsePayload, JsonOptions);
            }
            catch (JsonException)
            {
                return Failure(BankIntegrationErrorCodes.BankBadResponse, "External bank returned invalid JSON.", requestUrl, requestPayload, responsePayload, response, stopwatch.ElapsedMilliseconds);
            }

            if (bankResponse is null)
            {
                return Failure(BankIntegrationErrorCodes.BankBadResponse, "External bank returned an empty response.", requestUrl, requestPayload, responsePayload, response, stopwatch.ElapsedMilliseconds);
            }

            return new ExternalBankCallResult(
                bankResponse.IsValid,
                bankResponse.IsValid ? null : bankResponse.ErrorCode ?? BankIntegrationErrorCodes.ValidationRejected,
                bankResponse.Message ?? (bankResponse.IsValid ? "Bank validation succeeded." : "Bank validation rejected."),
                requestUrl,
                requestPayload,
                responsePayload,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return new ExternalBankCallResult(false, BankIntegrationErrorCodes.BankTimeout, "External bank request timed out.", requestUrl, requestPayload, null, null, stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException exception)
        {
            stopwatch.Stop();
            return new ExternalBankCallResult(false, BankIntegrationErrorCodes.BankUnavailable, exception.Message, requestUrl, requestPayload, null, null, stopwatch.ElapsedMilliseconds);
        }
    }

    private static ExternalBankCallResult Failure(string errorCode, string message, string requestUrl, string requestPayload, string? responsePayload, HttpResponseMessage response, long durationMs)
    {
        return new ExternalBankCallResult(false, errorCode, message, requestUrl, requestPayload, responsePayload, (int)response.StatusCode, durationMs);
    }
}

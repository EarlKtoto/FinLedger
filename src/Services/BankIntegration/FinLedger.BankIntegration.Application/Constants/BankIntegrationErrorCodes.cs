namespace FinLedger.BankIntegration.Application.Constants;

public static class BankIntegrationErrorCodes
{
    public const string BankConnectionNotFound = "BANK_CONNECTION_NOT_FOUND";
    public const string BankConnectionInactive = "BANK_CONNECTION_INACTIVE";
    public const string BankTimeout = "BANK_TIMEOUT";
    public const string BankUnavailable = "BANK_UNAVAILABLE";
    public const string BankUnauthorized = "BANK_UNAUTHORIZED";
    public const string BankBadResponse = "BANK_BAD_RESPONSE";
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
    public const string CurrencyNotSupported = "CURRENCY_NOT_SUPPORTED";
    public const string LimitExceeded = "LIMIT_EXCEEDED";
    public const string ValidationRejected = "VALIDATION_REJECTED";
    public const string UnknownBankError = "UNKNOWN_BANK_ERROR";

    public static readonly string[] BankProvidedCodes =
    [
        AccountNotFound,
        AccountInactive,
        InsufficientFunds,
        CurrencyNotSupported,
        LimitExceeded,
        ValidationRejected,
        UnknownBankError
    ];
}

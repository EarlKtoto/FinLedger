namespace FinLedger.Ledger.Application.Constants;

public static class LedgerPermissions
{
    public const string LedgerAccountsRead = "ledger-accounts:read";
    public const string LedgerAccountsCreate = "ledger-accounts:create";
    public const string BalancesRead = "balances:read";
    public const string FundsReserve = "funds:reserve";
    public const string FundsRelease = "funds:release";
    public const string FundsCapture = "funds:capture";
    public const string TransfersCreate = "transfers:create";
    public const string LedgerTransactionsRead = "ledger-transactions:read";
    public const string LedgerTransactionsReverse = "ledger-transactions:reverse";

    public static readonly string[] All =
    [
        LedgerAccountsRead,
        LedgerAccountsCreate,
        BalancesRead,
        FundsReserve,
        FundsRelease,
        FundsCapture,
        TransfersCreate,
        LedgerTransactionsRead,
        LedgerTransactionsReverse
    ];
}

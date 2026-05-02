# Ledger EF Core migrations

Initial schema migration creates:

- `LedgerAccounts`
- `AccountBalances`
- `LedgerTransactions`
- `LedgerEntries`
- `FundsReservations`

`AccountBalances.Version` is configured as a SQL Server `rowversion` concurrency token.

Apply locally with:

```powershell
dotnet ef database update --project .\src\Services\Ledger\FinLedger.Ledger.Infrastructure --startup-project .\src\Services\Ledger\FinLedger.Ledger.Api
```

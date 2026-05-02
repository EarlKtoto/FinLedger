# Transactions EF Core migrations

Initial schema migration creates:

- `Transactions`
- `TransactionHistory`

Apply locally with:

```powershell
dotnet ef database update --project .\src\Services\Transactions\FinLedger.Transactions.Infrastructure --startup-project .\src\Services\Transactions\FinLedger.Transactions.Api
```

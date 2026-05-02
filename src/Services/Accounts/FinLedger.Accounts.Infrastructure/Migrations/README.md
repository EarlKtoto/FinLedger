# Accounts EF Core migrations

Initial schema migration creates:

- `Accounts`
- `AccountLimits`
- `AccountStatusHistory`
- `AccountNumberSequences`

Apply locally with:

```powershell
dotnet ef database update --project .\src\Services\Accounts\FinLedger.Accounts.Infrastructure --startup-project .\src\Services\Accounts\FinLedger.Accounts.Api
```

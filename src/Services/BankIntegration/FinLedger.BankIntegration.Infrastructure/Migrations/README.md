# BankIntegration EF Core migrations

Initial schema migration creates:

- `BankConnections`
- `BankIntegrationRequestLogs`

Apply locally with:

```powershell
dotnet ef database update --project .\src\Services\BankIntegration\FinLedger.BankIntegration.Infrastructure --startup-project .\src\Services\BankIntegration\FinLedger.BankIntegration.Api
```

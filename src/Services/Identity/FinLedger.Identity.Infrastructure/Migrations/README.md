# Identity EF Core migrations

`20260426170000_InitialIdentitySchema` creates ASP.NET Core Identity tables plus `RefreshTokens`, `ApiClients`, `ApiKeys`, and `LoginAudits`.

To add future migrations, install or use `dotnet-ef`, then run:

```powershell
dotnet ef migrations add <Name> --project src/Services/Identity/FinLedger.Identity.Infrastructure --startup-project src/Services/Identity/FinLedger.Identity.Api
```

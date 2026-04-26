namespace FinLedger.Contracts.Audit;

public sealed record CreateAuditRecordRequest(string SourceService, string Operation, string Outcome, string Details);

public sealed record AuditRecordDto(Guid Id, string SourceService, string Operation, string Outcome, string Details, DateTimeOffset CreatedAt);

public sealed record AuditEventDto(Guid Id, string SourceService, string Operation, string Outcome, string Details, DateTimeOffset CreatedAt);
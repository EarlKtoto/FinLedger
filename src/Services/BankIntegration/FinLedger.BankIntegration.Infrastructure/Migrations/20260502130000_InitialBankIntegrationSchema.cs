using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinLedger.BankIntegration.Infrastructure.Migrations;

public partial class InitialBankIntegrationSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BankConnections",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BankCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                BaseUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                ApiKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankConnections", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BankIntegrationRequestLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BankConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ValidationType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                RequestUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                RequestPayload = table.Column<string>(type: "nvarchar(4096)", maxLength: 4096, nullable: false),
                ResponsePayload = table.Column<string>(type: "nvarchar(4096)", maxLength: 4096, nullable: true),
                HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                Succeeded = table.Column<bool>(type: "bit", nullable: false),
                ErrorCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                ErrorMessage = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                DurationMs = table.Column<long>(type: "bigint", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankIntegrationRequestLogs", x => x.Id);
                table.ForeignKey("FK_BankIntegrationRequestLogs_BankConnections_BankConnectionId", x => x.BankConnectionId, "BankConnections", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_BankConnections_BankCode", "BankConnections", "BankCode");
        migrationBuilder.CreateIndex("IX_BankConnections_ParticipantId", "BankConnections", "ParticipantId", unique: true);
        migrationBuilder.CreateIndex("IX_BankIntegrationRequestLogs_BankConnectionId", "BankIntegrationRequestLogs", "BankConnectionId");
        migrationBuilder.CreateIndex("IX_BankIntegrationRequestLogs_CreatedAtUtc", "BankIntegrationRequestLogs", "CreatedAtUtc");
        migrationBuilder.CreateIndex("IX_BankIntegrationRequestLogs_ParticipantId", "BankIntegrationRequestLogs", "ParticipantId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("BankIntegrationRequestLogs");
        migrationBuilder.DropTable("BankConnections");
    }
}

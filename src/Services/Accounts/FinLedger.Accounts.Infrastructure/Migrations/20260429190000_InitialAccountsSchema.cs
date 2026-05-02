using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinLedger.Accounts.Infrastructure.Migrations;

public partial class InitialAccountsSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                AllowIncomingPayments = table.Column<bool>(type: "bit", nullable: false),
                AllowOutgoingPayments = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ClosedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Accounts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AccountNumberSequences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Prefix = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                NextValue = table.Column<long>(type: "bigint", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountNumberSequences", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AccountLimits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LimitType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountLimits", x => x.Id);
                table.ForeignKey("FK_AccountLimits_Accounts_AccountId", x => x.AccountId, "Accounts", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AccountStatusHistory",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PreviousStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                NewStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Reason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountStatusHistory", x => x.Id);
                table.ForeignKey("FK_AccountStatusHistory_Accounts_AccountId", x => x.AccountId, "Accounts", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_AccountLimits_AccountId_LimitType", "AccountLimits", new[] { "AccountId", "LimitType" }, unique: true);
        migrationBuilder.CreateIndex("IX_AccountNumberSequences_Prefix", "AccountNumberSequences", "Prefix", unique: true);
        migrationBuilder.CreateIndex("IX_Accounts_AccountNumber", "Accounts", "AccountNumber", unique: true);
        migrationBuilder.CreateIndex("IX_Accounts_ParticipantId", "Accounts", "ParticipantId");
        migrationBuilder.CreateIndex("IX_AccountStatusHistory_AccountId", "AccountStatusHistory", "AccountId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AccountLimits");
        migrationBuilder.DropTable("AccountNumberSequences");
        migrationBuilder.DropTable("AccountStatusHistory");
        migrationBuilder.DropTable("Accounts");
    }
}

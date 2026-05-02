using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinLedger.Ledger.Infrastructure.Migrations;

public partial class InitialLedgerSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LedgerAccounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AccountNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LedgerAccounts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "LedgerTransactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalTransactionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                FailedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ReversedTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LedgerTransactions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AccountBalances",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AvailableBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                ReservedBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountBalances", x => x.Id);
                table.ForeignKey("FK_AccountBalances_LedgerAccounts_LedgerAccountId", x => x.LedgerAccountId, "LedgerAccounts", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FundsReservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalTransactionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ConfirmedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ReleasedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FundsReservations", x => x.Id);
                table.ForeignKey("FK_FundsReservations_LedgerAccounts_LedgerAccountId", x => x.LedgerAccountId, "LedgerAccounts", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "LedgerEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LedgerTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Direction = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LedgerEntries", x => x.Id);
                table.ForeignKey("FK_LedgerEntries_LedgerAccounts_LedgerAccountId", x => x.LedgerAccountId, "LedgerAccounts", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_LedgerEntries_LedgerTransactions_LedgerTransactionId", x => x.LedgerTransactionId, "LedgerTransactions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_AccountBalances_LedgerAccountId", "AccountBalances", "LedgerAccountId", unique: true);
        migrationBuilder.CreateIndex("IX_FundsReservations_ExternalTransactionId", "FundsReservations", "ExternalTransactionId");
        migrationBuilder.CreateIndex("IX_FundsReservations_LedgerAccountId", "FundsReservations", "LedgerAccountId");
        migrationBuilder.CreateIndex("IX_LedgerAccounts_AccountId", "LedgerAccounts", "AccountId", unique: true);
        migrationBuilder.CreateIndex("IX_LedgerEntries_LedgerAccountId", "LedgerEntries", "LedgerAccountId");
        migrationBuilder.CreateIndex("IX_LedgerEntries_LedgerTransactionId", "LedgerEntries", "LedgerTransactionId");
        migrationBuilder.CreateIndex("IX_LedgerTransactions_ExternalTransactionId", "LedgerTransactions", "ExternalTransactionId");
        migrationBuilder.CreateIndex("IX_LedgerTransactions_IdempotencyKey", "LedgerTransactions", "IdempotencyKey", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AccountBalances");
        migrationBuilder.DropTable("FundsReservations");
        migrationBuilder.DropTable("LedgerEntries");
        migrationBuilder.DropTable("LedgerAccounts");
        migrationBuilder.DropTable("LedgerTransactions");
    }
}

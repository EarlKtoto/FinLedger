using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinLedger.Transactions.Infrastructure.Migrations;

public partial class InitialTransactionsSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TransactionNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                PayerParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReceiverParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReceiverAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayerBankCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                PayerAccountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                ReceiverBankCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                ReceiverAccountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                ExternalReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                LedgerReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LedgerTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                FailureReason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TransactionHistory",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TransactionHistory", x => x.Id);
                table.ForeignKey("FK_TransactionHistory_Transactions_TransactionId", x => x.TransactionId, "Transactions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_TransactionHistory_TransactionId", "TransactionHistory", "TransactionId");
        migrationBuilder.CreateIndex("IX_Transactions_ExternalReference", "Transactions", "ExternalReference");
        migrationBuilder.CreateIndex("IX_Transactions_PayerParticipantId", "Transactions", "PayerParticipantId");
        migrationBuilder.CreateIndex("IX_Transactions_ReceiverParticipantId", "Transactions", "ReceiverParticipantId");
        migrationBuilder.CreateIndex("IX_Transactions_Status", "Transactions", "Status");
        migrationBuilder.CreateIndex("IX_Transactions_TransactionNumber", "Transactions", "TransactionNumber", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("TransactionHistory");
        migrationBuilder.DropTable("Transactions");
    }
}

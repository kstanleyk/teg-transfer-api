using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Reservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ledger_wallet_id_timestamp",
                schema: "core",
                table: "ledger");

            migrationBuilder.AddColumn<DateTime>(
                name: "processed_at",
                schema: "core",
                table: "ledger",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "processed_by",
                schema: "core",
                table: "ledger",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "purchase_reservation_id",
                schema: "core",
                table: "ledger",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "purchase_reservations",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_fee_ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PurchaseCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ServiceFeeAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ServiceFeeCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    supplier_details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    processed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_reservations_wallet_set_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "core",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_purchase_reservation_id",
                schema: "core",
                table: "ledger",
                column: "purchase_reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_reference",
                schema: "core",
                table: "ledger",
                column: "reference");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_status",
                schema: "core",
                table: "ledger",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_type",
                schema: "core",
                table: "ledger",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_client_id",
                schema: "core",
                table: "purchase_reservations",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_created_at",
                schema: "core",
                table: "purchase_reservations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_purchase_ledger_id",
                schema: "core",
                table: "purchase_reservations",
                column: "purchase_ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_service_fee_ledger_id",
                schema: "core",
                table: "purchase_reservations",
                column: "service_fee_ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_status",
                schema: "core",
                table: "purchase_reservations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_wallet_id",
                schema: "core",
                table: "purchase_reservations",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_reservations_wallet_id_status",
                schema: "core",
                table: "purchase_reservations",
                columns: new[] { "wallet_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "fk_ledger_purchase_reservation_set_purchase_reservation_id",
                schema: "core",
                table: "ledger",
                column: "purchase_reservation_id",
                principalSchema: "core",
                principalTable: "purchase_reservations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ledger_purchase_reservation_set_purchase_reservation_id",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropTable(
                name: "purchase_reservations",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "ix_ledger_purchase_reservation_id",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropIndex(
                name: "ix_ledger_reference",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropIndex(
                name: "ix_ledger_status",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropIndex(
                name: "ix_ledger_type",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "processed_at",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "processed_by",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "purchase_reservation_id",
                schema: "core",
                table: "ledger");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_wallet_id_timestamp",
                schema: "core",
                table: "ledger",
                columns: new[] { "wallet_id", "timestamp" });
        }
    }
}

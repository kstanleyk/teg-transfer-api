using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRateHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rate_history",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "rate_lock",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exchange_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    target_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    locked_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    locked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    lock_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rate_lock", x => x.id);
                    table.ForeignKey(
                        name: "fk_rate_lock_client_client_id",
                        column: x => x.client_id,
                        principalSchema: "identity",
                        principalTable: "client",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_rate_lock_exchange_rate_exchange_rate_id",
                        column: x => x.exchange_rate_id,
                        principalSchema: "core",
                        principalTable: "exchange_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_rate_locks_client_active",
                schema: "core",
                table: "rate_lock",
                columns: new[] { "client_id", "is_used", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_rate_locks_client_currency_active",
                schema: "core",
                table: "rate_lock",
                columns: new[] { "client_id", "base_currency", "target_currency", "is_used", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_rate_locks_client_id",
                schema: "core",
                table: "rate_lock",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_rate_locks_currency_pair",
                schema: "core",
                table: "rate_lock",
                columns: new[] { "base_currency", "target_currency" })
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "ix_rate_locks_exchange_rate_id",
                schema: "core",
                table: "rate_lock",
                column: "exchange_rate_id");

            migrationBuilder.CreateIndex(
                name: "ix_rate_locks_valid_until",
                schema: "core",
                table: "rate_lock",
                column: "valid_until");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rate_lock",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "exchange_rate_history",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    change_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    exchange_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    new_base_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_effective_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_margin = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    new_market_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_target_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_base_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_effective_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_margin = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    previous_market_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_target_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rate_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_exchange_rate_history_exchange_rate_exchange_rate_id",
                        column: x => x.exchange_rate_id,
                        principalSchema: "core",
                        principalTable: "exchange_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rate_history_exchange_rate_id",
                schema: "core",
                table: "exchange_rate_history",
                column: "exchange_rate_id");
        }
    }
}

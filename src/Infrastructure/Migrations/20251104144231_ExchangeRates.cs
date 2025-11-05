using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExchangeRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_group",
                schema: "identity",
                table: "client",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exchange_rate",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    target_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    base_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    target_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    margin = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    client_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rate", x => x.id);
                    table.CheckConstraint("CK_ExchangeRates_BaseCurrencyValue", "[BaseCurrencyValue] > 0");
                    table.CheckConstraint("CK_ExchangeRates_EffectiveDates", "[EffectiveTo] IS NULL OR [EffectiveTo] > [EffectiveFrom]");
                    table.CheckConstraint("CK_ExchangeRates_Margin", "[Margin] >= 0 AND [Margin] <= 1");
                    table.CheckConstraint("CK_ExchangeRates_RateTypeConstraints", "([Type] = 'General' AND [ClientId] IS NULL AND [ClientGroup] IS NULL) OR ([Type] = 'Group' AND [ClientId] IS NULL AND [ClientGroup] IS NOT NULL) OR ([Type] = 'Individual' AND [ClientId] IS NOT NULL AND [ClientGroup] IS NULL)");
                    table.CheckConstraint("CK_ExchangeRates_ReasonableValues", "[BaseCurrencyValue] < 1000 AND [TargetCurrencyValue] < 1000");
                    table.CheckConstraint("CK_ExchangeRates_TargetCurrencyValue", "[TargetCurrencyValue] > 0");
                },
                comment: "Stores exchange rates with hierarchical application: Individual -> Group -> General");

            migrationBuilder.CreateTable(
                name: "exchange_rate_history",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exchange_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_base_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_base_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_target_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_target_currency_value = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_margin = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    new_margin = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    previous_market_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_market_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    previous_effective_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    new_effective_rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    change_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rate_history", x => x.id);
                    table.CheckConstraint("CK_ExchangeRateHistories_PositiveValues", "[PreviousBaseCurrencyValue] > 0 AND [NewBaseCurrencyValue] > 0 AND [PreviousTargetCurrencyValue] > 0 AND [NewTargetCurrencyValue] > 0");
                    table.CheckConstraint("CK_ExchangeRateHistories_ValidMargins", "[PreviousMargin] >= 0 AND [PreviousMargin] <= 1 AND [NewMargin] >= 0 AND [NewMargin] <= 1");
                    table.CheckConstraint("CK_ExchangeRateHistories_ValidRates", "[PreviousMarketRate] > 0 AND [NewMarketRate] > 0 AND [PreviousEffectiveRate] > 0 AND [NewEffectiveRate] > 0");
                    table.ForeignKey(
                        name: "fk_exchange_rate_history_exchange_rate_exchange_rate_id",
                        column: x => x.exchange_rate_id,
                        principalSchema: "core",
                        principalTable: "exchange_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Audit trail for all changes to exchange rates, storing before and after values");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientGroup",
                schema: "identity",
                table: "client",
                column: "client_group",
                filter: "[ClientGroup] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status_Group",
                schema: "identity",
                table: "client",
                columns: new[] { "status", "client_group" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_ClientRates",
                schema: "core",
                table: "exchange_rate",
                columns: new[] { "base_currency", "target_currency", "client_id", "is_active" },
                filter: "[ClientId] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_DateRange",
                schema: "core",
                table: "exchange_rate",
                columns: new[] { "effective_from", "effective_to" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_GeneralRates",
                schema: "core",
                table: "exchange_rate",
                columns: new[] { "base_currency", "target_currency", "type", "is_active" },
                filter: "[Type] = 'General' AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_GroupRates",
                schema: "core",
                table: "exchange_rate",
                columns: new[] { "base_currency", "target_currency", "client_group", "is_active" },
                filter: "[ClientGroup] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_RateResolution",
                schema: "core",
                table: "exchange_rate",
                columns: new[] { "base_currency", "target_currency", "type", "is_active", "effective_from", "effective_to" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Type_CreatedAt",
                schema: "core",
                table: "exchange_rate",
                columns: new[] { "type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHistories_ChangedAt",
                schema: "core",
                table: "exchange_rate_history",
                column: "changed_at");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHistories_ChangedBy_Date",
                schema: "core",
                table: "exchange_rate_history",
                columns: new[] { "changed_by", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHistories_ChangeType_Date",
                schema: "core",
                table: "exchange_rate_history",
                columns: new[] { "change_type", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHistories_ExchangeRateId",
                schema: "core",
                table: "exchange_rate_history",
                column: "exchange_rate_id");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHistories_RateId_ChangedAt",
                schema: "core",
                table: "exchange_rate_history",
                columns: new[] { "exchange_rate_id", "changed_at" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rate_history",
                schema: "core");

            migrationBuilder.DropTable(
                name: "exchange_rate",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ClientGroup",
                schema: "identity",
                table: "client");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Status_Group",
                schema: "identity",
                table: "client");

            migrationBuilder.DropColumn(
                name: "client_group",
                schema: "identity",
                table: "client");
        }
    }
}

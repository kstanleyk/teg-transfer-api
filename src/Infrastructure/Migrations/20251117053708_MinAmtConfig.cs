using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MinAmtConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exchange_rate_tier",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exchange_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    max_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    margin = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rate_tier", x => x.id);
                    table.ForeignKey(
                        name: "fk_exchange_rate_tier_exchange_rate_exchange_rate_id",
                        column: x => x.exchange_rate_id,
                        principalSchema: "core",
                        principalTable: "exchange_rate",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "minimum_amount_configuration",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    target_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    minimum_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_minimum_amount_configuration", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rate_tier_exchange_rate_id",
                schema: "core",
                table: "exchange_rate_tier",
                column: "exchange_rate_id");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rate_tier_exchange_rate_id_min_amount_max_amount",
                schema: "core",
                table: "exchange_rate_tier",
                columns: new[] { "exchange_rate_id", "min_amount", "max_amount" });

            migrationBuilder.CreateIndex(
                name: "ix_minimum_amount_configuration_base_currency_target_currency_",
                schema: "core",
                table: "minimum_amount_configuration",
                columns: new[] { "base_currency", "target_currency", "is_active" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_minimum_amount_configuration_is_active_effective_from_effec",
                schema: "core",
                table: "minimum_amount_configuration",
                columns: new[] { "is_active", "effective_from", "effective_to" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rate_tier",
                schema: "core");

            migrationBuilder.DropTable(
                name: "minimum_amount_configuration",
                schema: "core");
        }
    }
}

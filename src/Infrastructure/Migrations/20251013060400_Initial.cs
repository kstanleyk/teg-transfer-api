using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "client",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    feature = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_basic = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    profile_image_url = table.Column<string>(type: "character varying(355)", maxLength: 355, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    available_balance_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    available_balance_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_client_client_id",
                        column: x => x.client_id,
                        principalSchema: "core",
                        principalTable: "client",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                schema: "auth",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<string>(type: "character varying(150)", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permission", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalSchema: "auth",
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservation",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_fee_ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_amount_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    purchase_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    service_fee_amount_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    service_fee_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    total_amount_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
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
                    table.PrimaryKey("pk_reservation", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservation_wallet_set_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "core",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    completion_type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: ""),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger", x => x.id);
                    table.ForeignKey(
                        name: "fk_ledger_purchase_reservation_set_reservation_id",
                        column: x => x.reservation_id,
                        principalSchema: "core",
                        principalTable: "reservation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ledger_wallet_set_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "core",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_client_email",
                schema: "core",
                table: "client",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_client_name",
                schema: "core",
                table: "client",
                columns: new[] { "first_name", "last_name" });

            migrationBuilder.CreateIndex(
                name: "ix_client_phone_number",
                schema: "core",
                table: "client",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "ix_client_status",
                schema: "core",
                table: "client",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_reference",
                schema: "core",
                table: "ledger",
                column: "reference");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_reservation_id",
                schema: "core",
                table: "ledger",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_status",
                schema: "core",
                table: "ledger",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_timestamp",
                schema: "core",
                table: "ledger",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_type",
                schema: "core",
                table: "ledger",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_wallet_id",
                schema: "core",
                table: "ledger",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_permission_feature_action",
                schema: "auth",
                table: "permission",
                columns: new[] { "feature", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reservation_client_id",
                schema: "core",
                table: "reservation",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_created_at",
                schema: "core",
                table: "reservation",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_purchase_ledger_id",
                schema: "core",
                table: "reservation",
                column: "purchase_ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_service_fee_ledger_id",
                schema: "core",
                table: "reservation",
                column: "service_fee_ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_status",
                schema: "core",
                table: "reservation",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_wallet_id",
                schema: "core",
                table: "reservation",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservation_wallet_id_status",
                schema: "core",
                table: "reservation",
                columns: new[] { "wallet_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_role_name",
                schema: "auth",
                table: "role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_permission_id",
                schema: "auth",
                table: "role_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                schema: "auth",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                schema: "auth",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_client_id",
                schema: "core",
                table: "wallet",
                column: "client_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ledger",
                schema: "core");

            migrationBuilder.DropTable(
                name: "role_permission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "reservation",
                schema: "core");

            migrationBuilder.DropTable(
                name: "permission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "core");

            migrationBuilder.DropTable(
                name: "client",
                schema: "core");
        }
    }
}

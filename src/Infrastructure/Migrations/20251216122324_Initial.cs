using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "kyc");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "client_group",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_attachment",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    public_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_category = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uploaded_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_attachment", x => x.id);
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
                name: "role",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
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
                name: "client",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    client_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client", x => x.id);
                    table.ForeignKey(
                        name: "fk_client_client_group_set_client_group_id",
                        column: x => x.client_group_id,
                        principalSchema: "core",
                        principalTable: "client_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "role_claim",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claim_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "identity",
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
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rate", x => x.id);
                    table.ForeignKey(
                        name: "fk_exchange_rate_client_client_id",
                        column: x => x.client_id,
                        principalSchema: "core",
                        principalTable: "client",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_exchange_rate_client_group_client_group_id",
                        column: x => x.client_group_id,
                        principalSchema: "core",
                        principalTable: "client_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "kyc_profile",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rejected_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kyc_profile", x => x.id);
                    table.UniqueConstraint("ak_kyc_profile_client_id", x => x.client_id);
                    table.ForeignKey(
                        name: "fk_kyc_profile_client_client_id",
                        column: x => x.client_id,
                        principalSchema: "core",
                        principalTable: "client",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_client_client_id",
                        column: x => x.client_id,
                        principalSchema: "core",
                        principalTable: "client",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    BalanceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AvailableBalanceAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    AvailableBalanceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
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
                name: "exchange_rate_tier",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exchange_rate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    max_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                        principalSchema: "core",
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

            migrationBuilder.CreateTable(
                name: "email_verification",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verification_details_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    verification_details_verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verification_details_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_details_verification_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verification_details_provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    verification_details_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_verification", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_verification_kyc_profile_client_id",
                        column: x => x.client_id,
                        principalSchema: "kyc",
                        principalTable: "kyc_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_document",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    front_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    back_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    selfie_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletion_reason = table.Column<string>(type: "text", nullable: true),
                    verification_details_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    verification_details_verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verification_details_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_details_verification_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verification_details_provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    verification_details_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_document_kyc_profile_client_id",
                        column: x => x.client_id,
                        principalSchema: "kyc",
                        principalTable: "kyc_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kyc_verification_history",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kyc_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    new_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    performed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kyc_verification_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_kyc_verification_history_kyc_profile_kyc_profile_id",
                        column: x => x.kyc_profile_id,
                        principalSchema: "kyc",
                        principalTable: "kyc_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phone_verification",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verification_details_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    verification_details_verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verification_details_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_details_verification_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verification_details_provider_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    verification_details_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_phone_verification", x => x.id);
                    table.ForeignKey(
                        name: "fk_phone_verification_kyc_profile_client_id",
                        column: x => x.client_id,
                        principalSchema: "kyc",
                        principalTable: "kyc_profile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claim_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login",
                schema: "identity",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_login", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_user_login_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_token", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_user_token_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
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
                name: "email_verification_attempts",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email_verification_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_verification_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_verification_attempts_email_verification_email_verifi",
                        column: x => x.email_verification_id,
                        principalSchema: "kyc",
                        principalTable: "email_verification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_document_attempts",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    identity_document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_document_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_document_attempts_identity_document_identity_docum",
                        column: x => x.identity_document_id,
                        principalSchema: "kyc",
                        principalTable: "identity_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phone_verification_attempts",
                schema: "kyc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone_verification_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_phone_verification_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_phone_verification_attempts_phone_verification_phone_verifi",
                        column: x => x.phone_verification_id,
                        principalSchema: "kyc",
                        principalTable: "phone_verification",
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
                        name: "fk_ledger_reservation_set_reservation_id",
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
                name: "ix_client_client_group_id",
                schema: "core",
                table: "client",
                column: "client_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroups_IsActive",
                schema: "core",
                table: "client_group",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGroups_Name",
                schema: "core",
                table: "client_group",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_document_type",
                schema: "core",
                table: "document_attachment",
                column: "document_type");

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_entity_id",
                schema: "core",
                table: "document_attachment",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_entity_id_entity_type_is_deleted",
                schema: "core",
                table: "document_attachment",
                columns: new[] { "entity_id", "entity_type", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_entity_id_entity_type_is_deleted_docume",
                schema: "core",
                table: "document_attachment",
                columns: new[] { "entity_id", "entity_type", "is_deleted", "document_type" });

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_entity_type",
                schema: "core",
                table: "document_attachment",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_entity_type_is_deleted_uploaded_at",
                schema: "core",
                table: "document_attachment",
                columns: new[] { "entity_type", "is_deleted", "uploaded_at" });

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_is_deleted",
                schema: "core",
                table: "document_attachment",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_uploaded_at",
                schema: "core",
                table: "document_attachment",
                column: "uploaded_at");

            migrationBuilder.CreateIndex(
                name: "ix_document_attachment_uploaded_by",
                schema: "core",
                table: "document_attachment",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_client_id",
                schema: "kyc",
                table: "email_verification",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_email",
                schema: "kyc",
                table: "email_verification",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_status",
                schema: "kyc",
                table: "email_verification",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_email_verification_attempts_email_verification_id",
                schema: "kyc",
                table: "email_verification_attempts",
                column: "email_verification_id");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rate_client_group_id",
                schema: "core",
                table: "exchange_rate",
                column: "client_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rate_client_id",
                schema: "core",
                table: "exchange_rate",
                column: "client_id");

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
                name: "ix_identity_document_client_id",
                schema: "kyc",
                table: "identity_document",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_document_client_id_status_expiry_date",
                schema: "kyc",
                table: "identity_document",
                columns: new[] { "client_id", "status", "expiry_date" });

            migrationBuilder.CreateIndex(
                name: "ix_identity_document_document_number",
                schema: "kyc",
                table: "identity_document",
                column: "document_number");

            migrationBuilder.CreateIndex(
                name: "ix_identity_document_expiry_date",
                schema: "kyc",
                table: "identity_document",
                column: "expiry_date");

            migrationBuilder.CreateIndex(
                name: "ix_identity_document_status",
                schema: "kyc",
                table: "identity_document",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_identity_document_type",
                schema: "kyc",
                table: "identity_document",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_identity_document_attempts_identity_document_id",
                schema: "kyc",
                table: "identity_document_attempts",
                column: "identity_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_kyc_profile_client_id",
                schema: "kyc",
                table: "kyc_profile",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kyc_profile_expires_at",
                schema: "kyc",
                table: "kyc_profile",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_kyc_profile_status",
                schema: "kyc",
                table: "kyc_profile",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_kyc_verification_history_kyc_profile_id",
                schema: "kyc",
                table: "kyc_verification_history",
                column: "kyc_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_kyc_verification_history_kyc_profile_id_performed_at",
                schema: "kyc",
                table: "kyc_verification_history",
                columns: new[] { "kyc_profile_id", "performed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_kyc_verification_history_performed_at",
                schema: "kyc",
                table: "kyc_verification_history",
                column: "performed_at");

            migrationBuilder.CreateIndex(
                name: "ix_kyc_verification_history_performed_by",
                schema: "kyc",
                table: "kyc_verification_history",
                column: "performed_by");

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
                name: "ix_minimum_amount_configuration_base_currency_target_currency_",
                schema: "core",
                table: "minimum_amount_configuration",
                columns: new[] { "base_currency", "target_currency", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_minimum_amount_configuration_is_active_effective_from_effec",
                schema: "core",
                table: "minimum_amount_configuration",
                columns: new[] { "is_active", "effective_from", "effective_to" });

            migrationBuilder.CreateIndex(
                name: "ix_permission_feature_action",
                schema: "auth",
                table: "permission",
                columns: new[] { "feature", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_phone_verification_client_id",
                schema: "kyc",
                table: "phone_verification",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_phone_verification_phone_number",
                schema: "kyc",
                table: "phone_verification",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "ix_phone_verification_status",
                schema: "kyc",
                table: "phone_verification",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_phone_verification_attempts_phone_verification_id",
                schema: "kyc",
                table: "phone_verification_attempts",
                column: "phone_verification_id");

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
                name: "RoleNameIndex",
                schema: "identity",
                table: "role",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_claim_role_id",
                schema: "identity",
                table: "role_claim",
                column: "role_id");

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
                name: "EmailIndex",
                schema: "identity",
                table: "user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_user_client_id",
                schema: "identity",
                table: "user",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "user",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claim_user_id",
                schema: "identity",
                table: "user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_login_user_id",
                schema: "identity",
                table: "user_login",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id1",
                schema: "auth",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                schema: "identity",
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
                name: "document_attachment",
                schema: "core");

            migrationBuilder.DropTable(
                name: "email_verification_attempts",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "exchange_rate_tier",
                schema: "core");

            migrationBuilder.DropTable(
                name: "identity_document_attempts",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "kyc_verification_history",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "ledger",
                schema: "core");

            migrationBuilder.DropTable(
                name: "minimum_amount_configuration",
                schema: "core");

            migrationBuilder.DropTable(
                name: "phone_verification_attempts",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "rate_lock",
                schema: "core");

            migrationBuilder.DropTable(
                name: "role_claim",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "role_permission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_login",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_token",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "email_verification",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "identity_document",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "reservation",
                schema: "core");

            migrationBuilder.DropTable(
                name: "phone_verification",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "exchange_rate",
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
                name: "role",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "core");

            migrationBuilder.DropTable(
                name: "kyc_profile",
                schema: "kyc");

            migrationBuilder.DropTable(
                name: "client",
                schema: "core");

            migrationBuilder.DropTable(
                name: "client_group",
                schema: "core");
        }
    }
}

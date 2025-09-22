using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrovet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "branch",
                schema: "common",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    description = table.Column<string>(type: "character varying(75)", unicode: false, maxLength: 75, nullable: false),
                    address = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    telephone = table.Column<string>(type: "character varying(35)", unicode: false, maxLength: 35, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branch", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dialog_message",
                schema: "common",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    lid = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(125)", unicode: false, maxLength: 125, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dialog_message", x => new { x.id, x.lid });
                });

            migrationBuilder.CreateTable(
                name: "item",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(85)", maxLength: 85, nullable: false),
                    short_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    bar_code_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    min_stock = table.Column<double>(type: "double precision", nullable: false),
                    max_stock = table.Column<double>(type: "double precision", nullable: false),
                    reorder_lev = table.Column<double>(type: "double precision", nullable: false),
                    reorder_qtty = table.Column<double>(type: "double precision", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true, defaultValueSql: "gen_random_uuid()"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item_category",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true, defaultValueSql: "gen_random_uuid()"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item_movement",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    line_num = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    item = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trans_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    trans_time = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sense = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    qtty = table.Column<double>(type: "double precision", nullable: false),
                    source_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    source_line_num = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true, defaultValueSql: "gen_random_uuid()"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_movement", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "month_name",
                schema: "common",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    lid = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(125)", unicode: false, maxLength: 125, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_month_name", x => new { x.id, x.lid });
                });

            migrationBuilder.CreateTable(
                name: "order",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    order_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    supplier = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    trans_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true, defaultValueSql: "gen_random_uuid()"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
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
                name: "order_detail",
                schema: "inventory",
                columns: table => new
                {
                    line_num = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    order_id = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    item = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    receive_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qtty = table.Column<double>(type: "double precision", nullable: false),
                    unit_cost = table.Column<double>(type: "double precision", nullable: false),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    status = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    trans_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true, defaultValueSql: "gen_random_uuid()"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_detail", x => new { x.order_id, x.line_num });
                    table.ForeignKey(
                        name: "fk_order_detail_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "inventory",
                        principalTable: "order",
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

            migrationBuilder.CreateIndex(
                name: "ix_item_bar_code_text",
                schema: "inventory",
                table: "item",
                column: "bar_code_text",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_public_id",
                schema: "inventory",
                table: "item",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_category_name",
                schema: "inventory",
                table: "item_category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_category_public_id",
                schema: "inventory",
                table: "item_category",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_movement_public_id",
                schema: "inventory",
                table: "item_movement",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_movement_source_id_line_num_item",
                schema: "inventory",
                table: "item_movement",
                columns: new[] { "source_id", "line_num", "item" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_public_id",
                schema: "inventory",
                table: "order",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_public_id",
                schema: "inventory",
                table: "order_detail",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permission_feature_action",
                schema: "auth",
                table: "permission",
                columns: new[] { "feature", "action" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch",
                schema: "common");

            migrationBuilder.DropTable(
                name: "dialog_message",
                schema: "common");

            migrationBuilder.DropTable(
                name: "item",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "item_category",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "item_movement",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "month_name",
                schema: "common");

            migrationBuilder.DropTable(
                name: "order_detail",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "role_permission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "order",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "permission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user",
                schema: "auth");
        }
    }
}

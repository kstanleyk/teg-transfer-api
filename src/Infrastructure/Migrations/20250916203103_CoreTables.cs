using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unicorn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "department",
                schema: "core");

            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.CreateTable(
                name: "average_weight",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    estate = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    block = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    weight = table.Column<double>(type: "double precision", nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_average_weight", x => new { x.id, x.estate, x.block });
                });

            migrationBuilder.CreateTable(
                name: "block",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    estate = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    tree_number = table.Column<double>(type: "double precision", nullable: false),
                    date_established = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    block_size = table.Column<double>(type: "double precision", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_block", x => x.id);
                });

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
                name: "estate",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    location = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    date_established = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estate", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estate_task",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    task_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    estate_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rate = table.Column<double>(type: "double precision", nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estate_task", x => new { x.id, x.task_id, x.estate_id });
                });

            migrationBuilder.CreateTable(
                name: "estate_task_type",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    task_type_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    estate_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    account_id = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estate_task_type", x => new { x.id, x.task_type_id, x.estate_id });
                });

            migrationBuilder.CreateTable(
                name: "expense_source",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(35)", unicode: false, maxLength: 35, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_source", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expense_status",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expense_type",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    account = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    inventory_status = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expense_type_inventory",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    expense_type = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    inventory_item = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_type_inventory", x => new { x.id, x.expense_type, x.inventory_item });
                });

            migrationBuilder.CreateTable(
                name: "harvest_config",
                schema: "core",
                columns: table => new
                {
                    harvest_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    carrying_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_harvest_config", x => new { x.harvest_id, x.carrying_id });
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
                name: "operation",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    line = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    payroll = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    employee = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    estate = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    block = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    item = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    milling_cycle = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    description = table.Column<string>(type: "character varying(75)", unicode: false, maxLength: 75, nullable: false),
                    quantity = table.Column<double>(type: "double precision", nullable: false),
                    rate = table.Column<double>(type: "double precision", nullable: false),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    trans_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(2)", unicode: false, maxLength: 2, nullable: false),
                    sync_reference = table.Column<string>(type: "character varying(40)", unicode: false, maxLength: 40, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operation", x => new { x.id, x.line, x.payroll });
                });

            migrationBuilder.CreateTable(
                name: "payroll",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    pay_month = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    pay_period = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    trans_year = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    days_in_month = table.Column<double>(type: "double precision", nullable: false),
                    payroll_days = table.Column<double>(type: "double precision", nullable: false),
                    remark = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payroll", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_average_weight",
                schema: "core",
                columns: table => new
                {
                    payroll_id = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    estate_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    block_id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    average_fruit_bunch_weight = table.Column<double>(type: "double precision", unicode: false, maxLength: 5, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payroll_average_weight", x => new { x.payroll_id, x.estate_id, x.block_id });
                });

            migrationBuilder.CreateTable(
                name: "plant",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    block = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    number = table.Column<double>(type: "double precision", nullable: false),
                    trans_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    task_type = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_type",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false),
                    description = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    account = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_type_account",
                schema: "core",
                columns: table => new
                {
                    task_type = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    estate = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: false),
                    account = table.Column<string>(type: "character varying(15)", unicode: false, maxLength: 15, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_type_account", x => new { x.task_type, x.estate });
                });

            migrationBuilder.CreateIndex(
                name: "ix_average_weight_public_id",
                schema: "core",
                table: "average_weight",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_block_public_id",
                schema: "core",
                table: "block",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estate_public_id",
                schema: "core",
                table: "estate",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estate_task_type_public_id",
                schema: "core",
                table: "estate_task_type",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expense_source_public_id",
                schema: "core",
                table: "expense_source",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expense_status_public_id",
                schema: "core",
                table: "expense_status",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expense_type_public_id",
                schema: "core",
                table: "expense_type",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expense_type_inventory_public_id",
                schema: "core",
                table: "expense_type_inventory",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payroll_public_id",
                schema: "core",
                table: "payroll",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plant_public_id",
                schema: "core",
                table: "plant",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_task_public_id",
                schema: "core",
                table: "task",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_task_type_public_id",
                schema: "core",
                table: "task_type",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "average_weight",
                schema: "core");

            migrationBuilder.DropTable(
                name: "block",
                schema: "core");

            migrationBuilder.DropTable(
                name: "branch",
                schema: "common");

            migrationBuilder.DropTable(
                name: "dialog_message",
                schema: "common");

            migrationBuilder.DropTable(
                name: "estate",
                schema: "core");

            migrationBuilder.DropTable(
                name: "estate_task",
                schema: "core");

            migrationBuilder.DropTable(
                name: "estate_task_type",
                schema: "core");

            migrationBuilder.DropTable(
                name: "expense_source",
                schema: "core");

            migrationBuilder.DropTable(
                name: "expense_status",
                schema: "core");

            migrationBuilder.DropTable(
                name: "expense_type",
                schema: "core");

            migrationBuilder.DropTable(
                name: "expense_type_inventory",
                schema: "core");

            migrationBuilder.DropTable(
                name: "harvest_config",
                schema: "core");

            migrationBuilder.DropTable(
                name: "month_name",
                schema: "common");

            migrationBuilder.DropTable(
                name: "operation",
                schema: "core");

            migrationBuilder.DropTable(
                name: "payroll",
                schema: "core");

            migrationBuilder.DropTable(
                name: "payroll_average_weight",
                schema: "core");

            migrationBuilder.DropTable(
                name: "plant",
                schema: "core");

            migrationBuilder.DropTable(
                name: "task",
                schema: "core");

            migrationBuilder.DropTable(
                name: "task_type",
                schema: "core");

            migrationBuilder.DropTable(
                name: "task_type_account",
                schema: "core");

            migrationBuilder.CreateTable(
                name: "department",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    faculty_id = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", unicode: false, maxLength: 150, nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_department_public_id",
                schema: "core",
                table: "department",
                column: "public_id",
                unique: true);
        }
    }
}

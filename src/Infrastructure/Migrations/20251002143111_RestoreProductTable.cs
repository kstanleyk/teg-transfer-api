using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrovet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    brand_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    packaging_type_size_in_liters = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    packaging_type_units_per_box = table.Column<int>(type: "integer", nullable: false),
                    packaging_type_display_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    min_stock = table.Column<double>(type: "double precision", nullable: false),
                    max_stock = table.Column<double>(type: "double precision", nullable: false),
                    reorder_lev = table.Column<double>(type: "double precision", nullable: false),
                    reorder_qtty = table.Column<double>(type: "double precision", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product",
                schema: "inventory");
        }
    }
}

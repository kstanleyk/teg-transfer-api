using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrovet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product",
                schema: "inventory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product",
                schema: "inventory",
                columns: table => new
                {
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    brand_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    max_stock = table.Column<double>(type: "double precision", precision: 15, scale: 4, nullable: false),
                    min_stock = table.Column<double>(type: "double precision", precision: 15, scale: 4, nullable: false),
                    reorder_lev = table.Column<double>(type: "double precision", precision: 15, scale: 4, nullable: false),
                    reorder_qtty = table.Column<double>(type: "double precision", precision: 15, scale: 4, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    packaging_type_display_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    packaging_type_size_in_liters = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    packaging_type_units_per_box = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_brand_code",
                schema: "inventory",
                table: "product",
                column: "brand_code");

            migrationBuilder.CreateIndex(
                name: "ix_product_brand_code_category",
                schema: "inventory",
                table: "product",
                columns: new[] { "brand_code", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_product_category",
                schema: "inventory",
                table: "product",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_product_category_status",
                schema: "inventory",
                table: "product",
                columns: new[] { "category", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_product_public_id",
                schema: "inventory",
                table: "product",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_sku",
                schema: "inventory",
                table: "product",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_status",
                schema: "inventory",
                table: "product",
                column: "status");
        }
    }
}

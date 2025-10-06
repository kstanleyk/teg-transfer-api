using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agrovet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "packaging_type_units_per_box",
                schema: "inventory",
                table: "product",
                newName: "units_per_box");

            migrationBuilder.RenameColumn(
                name: "packaging_type_size_in_liters",
                schema: "inventory",
                table: "product",
                newName: "pack_size_liters");

            migrationBuilder.RenameColumn(
                name: "packaging_type_display_name",
                schema: "inventory",
                table: "product",
                newName: "pack_display_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "units_per_box",
                schema: "inventory",
                table: "product",
                newName: "packaging_type_units_per_box");

            migrationBuilder.RenameColumn(
                name: "pack_size_liters",
                schema: "inventory",
                table: "product",
                newName: "packaging_type_size_in_liters");

            migrationBuilder.RenameColumn(
                name: "pack_display_name",
                schema: "inventory",
                table: "product",
                newName: "packaging_type_display_name");
        }
    }
}

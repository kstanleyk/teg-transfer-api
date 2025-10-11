using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApproveBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                schema: "core",
                table: "ledger",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approved_by",
                schema: "core",
                table: "ledger",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_at",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "approved_by",
                schema: "core",
                table: "ledger");
        }
    }
}

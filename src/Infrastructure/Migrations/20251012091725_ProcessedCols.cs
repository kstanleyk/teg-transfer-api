using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProcessedCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "processed_at",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "processed_by",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropColumn(
                name: "rejected_by",
                schema: "core",
                table: "ledger");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "processed_at",
                schema: "core",
                table: "ledger",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "processed_by",
                schema: "core",
                table: "ledger",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "rejected_at",
                schema: "core",
                table: "ledger",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejected_by",
                schema: "core",
                table: "ledger",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}

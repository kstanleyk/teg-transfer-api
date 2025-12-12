using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TegWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ledger_purchase_reservation_set_reservation_id",
                schema: "core",
                table: "ledger");

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

            migrationBuilder.AddForeignKey(
                name: "fk_ledger_reservation_set_reservation_id",
                schema: "core",
                table: "ledger",
                column: "reservation_id",
                principalSchema: "core",
                principalTable: "reservation",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ledger_reservation_set_reservation_id",
                schema: "core",
                table: "ledger");

            migrationBuilder.DropTable(
                name: "document_attachment",
                schema: "core");

            migrationBuilder.AddForeignKey(
                name: "fk_ledger_purchase_reservation_set_reservation_id",
                schema: "core",
                table: "ledger",
                column: "reservation_id",
                principalSchema: "core",
                principalTable: "reservation",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

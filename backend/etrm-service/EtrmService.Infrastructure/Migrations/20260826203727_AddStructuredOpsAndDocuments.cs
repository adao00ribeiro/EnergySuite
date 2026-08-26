using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredOpsAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "operations",
                type: "text",
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.AddColumn<Guid>(
                name: "linked_operation_id",
                table: "operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "document_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bucket_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    object_key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_attachments", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_attachments_entity_type_entity_id",
                table: "document_attachments",
                columns: new[] { "entity_type", "entity_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_attachments");

            migrationBuilder.DropColumn(
                name: "category",
                table: "operations");

            migrationBuilder.DropColumn(
                name: "linked_operation_id",
                table: "operations");
        }
    }
}

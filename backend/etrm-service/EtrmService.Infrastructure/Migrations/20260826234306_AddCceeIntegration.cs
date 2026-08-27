using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCceeIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ccee_comparisons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counterparty_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counterparty_ccee_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    period = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    backops_volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ccee_volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    difference = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ccee_comparisons", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ccee_comparisons");
        }
    }
}

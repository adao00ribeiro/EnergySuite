using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContractsAndReadjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "flexibility_margin",
                table: "contracts",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "price_index_type",
                table: "contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "contracts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "contract_amendments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    previous_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    new_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    previous_volume_mw_med = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    new_volume_mw_med = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_amendments", x => x.id);
                    table.ForeignKey(
                        name: "FK_contract_amendments_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price_index_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    index_type = table.Column<int>(type: "integer", nullable: false),
                    reference_month = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    monthly_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_index_values", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contract_amendments_contract_id",
                table: "contract_amendments",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_price_index_values_index_type_reference_month",
                table: "price_index_values",
                columns: new[] { "index_type", "reference_month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_amendments");

            migrationBuilder.DropTable(
                name: "price_index_values");

            migrationBuilder.DropColumn(
                name: "flexibility_margin",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "price_index_type",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "version",
                table: "contracts");
        }
    }
}

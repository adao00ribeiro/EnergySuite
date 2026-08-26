using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPluviaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "precipitation_scenarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    reference_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    horizon_days = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precipitation_scenarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "model_executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_executions", x => x.id);
                    table.ForeignKey(
                        name: "fk_precipitation_scenario_executions",
                        column: x => x.scenario_id,
                        principalTable: "precipitation_scenarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hydrological_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submarket = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    basin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value_mw_med = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    value_percentage_mlt = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    target_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hydrological_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_model_execution_results",
                        column: x => x.execution_id,
                        principalTable: "model_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hydrological_results_execution_id",
                table: "hydrological_results",
                column: "execution_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_executions_scenario_id",
                table: "model_executions",
                column: "scenario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hydrological_results");

            migrationBuilder.DropTable(
                name: "model_executions");

            migrationBuilder.DropTable(
                name: "precipitation_scenarios");
        }
    }
}

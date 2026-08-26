using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForecastMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForecastMetadatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    ReferenceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: false),
                    EnsembleMembers = table.Column<int>(type: "integer", nullable: false),
                    LakehousePath = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastMetadatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastMetadatas");
        }
    }
}

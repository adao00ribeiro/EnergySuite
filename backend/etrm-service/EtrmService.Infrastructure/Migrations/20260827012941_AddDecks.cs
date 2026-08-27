using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prospect_decks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    sequence_order = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prospect_decks", x => x.id);
                    table.ForeignKey(
                        name: "FK_prospect_decks_prospect_studies_study_id",
                        column: x => x.study_id,
                        principalTable: "prospect_studies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prospect_deck_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    deck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    change_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prospect_deck_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_prospect_deck_versions_prospect_decks_deck_id",
                        column: x => x.deck_id,
                        principalTable: "prospect_decks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prospect_deck_versions_deck_id",
                table: "prospect_deck_versions",
                column: "deck_id");

            migrationBuilder.CreateIndex(
                name: "IX_prospect_decks_study_id",
                table: "prospect_decks",
                column: "study_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prospect_deck_versions");

            migrationBuilder.DropTable(
                name: "prospect_decks");
        }
    }
}

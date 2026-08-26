using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtrmService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialRegistryAndPortfolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlendConfig",
                table: "precipitation_scenarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "precipitation_scenarios",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UploadUrl",
                table: "precipitation_scenarios",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "economic_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_economic_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    additional_characteristics = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    contact_general_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_legal_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_financial_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_phone1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_phone2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_phone3 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    responsible = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    corporate_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    trade_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state_registration = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    economic_activity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ccee_profile = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ccee_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ccee_acronym = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ccee_class = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address_zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    contact_general_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_legal_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_financial_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_phone1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_phone2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_phone3 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    economic_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_economic_group",
                        column: x => x.economic_group_id,
                        principalTable: "economic_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_companies_cnpj",
                table: "companies",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_economic_group_id",
                table: "companies",
                column: "economic_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_persons_cpf",
                table: "persons",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "portfolios");

            migrationBuilder.DropTable(
                name: "economic_groups");

            migrationBuilder.DropColumn(
                name: "BlendConfig",
                table: "precipitation_scenarios");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "precipitation_scenarios");

            migrationBuilder.DropColumn(
                name: "UploadUrl",
                table: "precipitation_scenarios");
        }
    }
}

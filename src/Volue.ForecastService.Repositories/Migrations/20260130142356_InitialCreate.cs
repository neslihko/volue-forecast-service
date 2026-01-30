using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Volue.ForecastService.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "power_plants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacity_mwh = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_power_plants", x => x.id);
                    table.ForeignKey(
                        name: "FK_power_plants_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forecasts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hour_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    mwh = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forecasts", x => x.id);
                    table.ForeignKey(
                        name: "FK_forecasts_power_plants_plant_id",
                        column: x => x.plant_id,
                        principalTable: "power_plants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "id", "created_at", "is_active", "name", "updated_at" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), true, "Energy Trading Corp", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "power_plants",
                columns: new[] { "id", "capacity_mwh", "company_id", "country", "created_at", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), 150.0000m, new Guid("11111111-1111-1111-1111-111111111111"), "Turkey", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), true, "Istanbul Wind Farm", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 200.0000m, new Guid("11111111-1111-1111-1111-111111111111"), "Bulgaria", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), true, "Sofia Solar Park", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 300.0000m, new Guid("11111111-1111-1111-1111-111111111111"), "Spain", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), true, "Madrid Hydro Station", new DateTime(2026, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "idx_forecasts_hour",
                table: "forecasts",
                column: "hour_utc");

            migrationBuilder.CreateIndex(
                name: "idx_forecasts_plant_hour",
                table: "forecasts",
                columns: new[] { "plant_id", "hour_utc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_power_plants_company_id",
                table: "power_plants",
                column: "company_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forecasts");

            migrationBuilder.DropTable(
                name: "power_plants");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}

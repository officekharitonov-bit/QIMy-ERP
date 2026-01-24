using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VatlayerApiIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "TaxRates",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "TaxRates",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "TaxRates",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveUntil",
                table: "TaxRates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TaxRates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RateType",
                table: "TaxRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "TaxRates",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VatRateChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false),
                    CountryName = table.Column<string>(type: "TEXT", nullable: false),
                    RateType = table.Column<int>(type: "INTEGER", nullable: false),
                    OldRate = table.Column<decimal>(type: "TEXT", nullable: true),
                    NewRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    IsNotified = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ChangedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatRateChangeLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VatRateChangeLogs");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "EffectiveUntil",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "RateType",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "TaxRates");
        }
    }
}

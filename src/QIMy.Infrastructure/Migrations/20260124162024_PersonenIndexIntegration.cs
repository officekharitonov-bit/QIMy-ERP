using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersonenIndexIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "TaxRates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    NameGerman = table.Column<string>(type: "TEXT", nullable: false),
                    NameEnglish = table.Column<string>(type: "TEXT", nullable: false),
                    CountryNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEuMember = table.Column<bool>(type: "INTEGER", nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EuCountryData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false),
                    PurchaseThreshold = table.Column<decimal>(type: "TEXT", nullable: false),
                    SupplyThreshold = table.Column<decimal>(type: "TEXT", nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CountryNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    LastVerified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuCountryData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EuCountryData_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_CountryId",
                table: "TaxRates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_EuCountryData_CountryId",
                table: "EuCountryData",
                column: "CountryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxRates_Countries_CountryId",
                table: "TaxRates",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxRates_Countries_CountryId",
                table: "TaxRates");

            migrationBuilder.DropTable(
                name: "EuCountryData");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_CountryId",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "TaxRates");
        }
    }
}

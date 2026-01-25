using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSteuercodeKontoProz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Konto",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Proz",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Steuercode",
                table: "Invoices",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Konto",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Proz",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Steuercode",
                table: "Invoices");
        }
    }
}

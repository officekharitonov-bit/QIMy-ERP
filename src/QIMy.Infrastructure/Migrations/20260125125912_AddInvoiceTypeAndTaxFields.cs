using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTypeAndTaxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "CashBookDays");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsIntraEUSale",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReverseCharge",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSmallBusinessExemption",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxFreeExport",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsIntraEUSale",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsReverseCharge",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsSmallBusinessExemption",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsTaxFreeExport",
                table: "Invoices");

            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "CashBookDays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}

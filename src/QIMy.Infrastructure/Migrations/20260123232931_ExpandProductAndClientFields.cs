using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProductAndClientFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalName",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartNumber",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TareUnit",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitsInTare",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BIC",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Clients",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField01",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField02",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField03",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField04",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField05",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField06",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPaymentMethodId",
                table: "Clients",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermsDays",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CurrencyId",
                table: "Clients",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DefaultPaymentMethodId",
                table: "Clients",
                column: "DefaultPaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Currencies_CurrencyId",
                table: "Clients",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_PaymentMethods_DefaultPaymentMethodId",
                table: "Clients",
                column: "DefaultPaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Currencies_CurrencyId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_PaymentMethods_DefaultPaymentMethodId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CurrencyId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DefaultPaymentMethodId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AdditionalName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TareUnit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitsInTare",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BIC",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CustomField01",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CustomField02",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CustomField03",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CustomField04",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CustomField05",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CustomField06",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DefaultPaymentMethodId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PaymentTermsDays",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Clients");
        }
    }
}

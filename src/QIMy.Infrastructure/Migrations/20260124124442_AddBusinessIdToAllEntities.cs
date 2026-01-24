using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessIdToAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Units",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "TaxRates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Suppliers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Payments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "PaymentMethods",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "NumberingConfigs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Discounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Currencies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "ClientTypes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "ClientAreas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_BusinessId",
                table: "Units",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_BusinessId",
                table: "TaxRates",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Suppliers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessId",
                table: "Products",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BusinessId",
                table: "Payments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_BusinessId",
                table: "PaymentMethods",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_NumberingConfigs_BusinessId",
                table: "NumberingConfigs",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_BusinessId",
                table: "Discounts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_BusinessId",
                table: "Currencies",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientTypes_BusinessId",
                table: "ClientTypes",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAreas_BusinessId",
                table: "ClientAreas",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_BusinessId",
                table: "Accounts",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Businesses_BusinessId",
                table: "Accounts",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientAreas_Businesses_BusinessId",
                table: "ClientAreas",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientTypes_Businesses_BusinessId",
                table: "ClientTypes",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Currencies_Businesses_BusinessId",
                table: "Currencies",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Businesses_BusinessId",
                table: "Discounts",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NumberingConfigs_Businesses_BusinessId",
                table: "NumberingConfigs",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_Businesses_BusinessId",
                table: "PaymentMethods",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Businesses_BusinessId",
                table: "Payments",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Businesses_BusinessId",
                table: "Products",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Businesses_BusinessId",
                table: "Suppliers",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxRates_Businesses_BusinessId",
                table: "TaxRates",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Businesses_BusinessId",
                table: "Units",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Businesses_BusinessId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientAreas_Businesses_BusinessId",
                table: "ClientAreas");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientTypes_Businesses_BusinessId",
                table: "ClientTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Currencies_Businesses_BusinessId",
                table: "Currencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Businesses_BusinessId",
                table: "Discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_NumberingConfigs_Businesses_BusinessId",
                table: "NumberingConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_Businesses_BusinessId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Businesses_BusinessId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Businesses_BusinessId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Businesses_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxRates_Businesses_BusinessId",
                table: "TaxRates");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Businesses_BusinessId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_BusinessId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_BusinessId",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Products_BusinessId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BusinessId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_BusinessId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_NumberingConfigs_BusinessId",
                table: "NumberingConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_BusinessId",
                table: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_BusinessId",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_ClientTypes_BusinessId",
                table: "ClientTypes");

            migrationBuilder.DropIndex(
                name: "IX_ClientAreas_BusinessId",
                table: "ClientAreas");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_BusinessId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "NumberingConfigs");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "ClientTypes");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "ClientAreas");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Accounts");
        }
    }
}

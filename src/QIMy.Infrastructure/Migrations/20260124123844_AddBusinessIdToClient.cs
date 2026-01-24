using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessIdToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Clients",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_BusinessId",
                table: "Clients",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Businesses_BusinessId",
                table: "Clients",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Businesses_BusinessId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_BusinessId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Clients");
        }
    }
}

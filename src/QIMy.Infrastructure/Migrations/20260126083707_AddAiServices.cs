using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableAutoOcr = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableAutoClassification = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableAutoApproval = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoApprovalThreshold = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MinConfidenceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    PreferredLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    EnableAnomalyDetection = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableChatAssistant = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiConfigurations_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiProcessingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExpenseInvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    ServiceType = table.Column<string>(type: "TEXT", nullable: false),
                    RawInput = table.Column<string>(type: "TEXT", nullable: true),
                    AiResponse = table.Column<string>(type: "TEXT", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    WasAcceptedByUser = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserCorrection = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessingTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProcessingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiProcessingLogs_ExpenseInvoices_ExpenseInvoiceId",
                        column: x => x.ExpenseInvoiceId,
                        principalTable: "ExpenseInvoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiProcessingLogs_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AiSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExpenseInvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    SuggestionType = table.Column<string>(type: "TEXT", nullable: false),
                    SuggestedValue = table.Column<string>(type: "TEXT", nullable: false),
                    Confidence = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    Reasoning = table.Column<string>(type: "TEXT", nullable: false),
                    WasAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Alternatives = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiSuggestions_ExpenseInvoices_ExpenseInvoiceId",
                        column: x => x.ExpenseInvoiceId,
                        principalTable: "ExpenseInvoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiSuggestions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnomalyAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExpenseInvoiceId = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Recommendation = table.Column<string>(type: "TEXT", nullable: false),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Resolution = table.Column<string>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnomalyAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnomalyAlerts_ExpenseInvoices_ExpenseInvoiceId",
                        column: x => x.ExpenseInvoiceId,
                        principalTable: "ExpenseInvoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnomalyAlerts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiConfigurations_BusinessId",
                table: "AiConfigurations",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiProcessingLogs_ExpenseInvoiceId",
                table: "AiProcessingLogs",
                column: "ExpenseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AiProcessingLogs_InvoiceId",
                table: "AiProcessingLogs",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AiSuggestions_ExpenseInvoiceId",
                table: "AiSuggestions",
                column: "ExpenseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AiSuggestions_InvoiceId",
                table: "AiSuggestions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyAlerts_ExpenseInvoiceId",
                table: "AnomalyAlerts",
                column: "ExpenseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyAlerts_InvoiceId",
                table: "AnomalyAlerts",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiConfigurations");

            migrationBuilder.DropTable(
                name: "AiProcessingLogs");

            migrationBuilder.DropTable(
                name: "AiSuggestions");

            migrationBuilder.DropTable(
                name: "AnomalyAlerts");
        }
    }
}

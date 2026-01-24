using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIMy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_AR_ER_KASSA_Integration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonenIndexEntryId",
                table: "Invoices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonenIndexEntryId",
                table: "ExpenseInvoices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankStatements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<int>(type: "INTEGER", nullable: false),
                    BankAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatementNumber = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalDebits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalCredits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReconciliedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Quarter = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatements_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankStatements_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashBoxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<int>(type: "INTEGER", nullable: false),
                    CashBoxNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CashBoxType = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBoxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashBoxes_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonenIndexEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<int>(type: "INTEGER", nullable: true),
                    KtoNr = table.Column<string>(type: "TEXT", nullable: false),
                    TAG = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false),
                    UIDNumber = table.Column<string>(type: "TEXT", nullable: true),
                    SuggestedExpenseAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    SuggestedIncomeAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    BranchNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    CountryNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    ContractorType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsInternalDivision = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: true),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: true),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonenIndexEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonenIndexEntries_Accounts_SuggestedExpenseAccountId",
                        column: x => x.SuggestedExpenseAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonenIndexEntries_Accounts_SuggestedIncomeAccountId",
                        column: x => x.SuggestedIncomeAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonenIndexEntries_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonenIndexEntries_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonenIndexEntries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonenIndexEntries_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankStatementLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BankStatementId = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TransactionCode = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "TEXT", nullable: true),
                    CounterpartyIban = table.Column<string>(type: "TEXT", nullable: true),
                    CounterpartyName = table.Column<string>(type: "TEXT", nullable: true),
                    LineSequence = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedDocumentId = table.Column<int>(type: "INTEGER", nullable: true),
                    LinkedDocumentType = table.Column<string>(type: "TEXT", nullable: true),
                    ReconciliationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatementLines_BankStatements_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashBookDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CashBoxId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalIncome = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalExpense = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ExpectedClosingBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ActualClosingBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Variance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Quarter = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBookDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashBookDays_CashBoxes_CashBoxId",
                        column: x => x.CashBoxId,
                        principalTable: "CashBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<int>(type: "INTEGER", nullable: false),
                    CashBoxId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntryNumber = table.Column<string>(type: "TEXT", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EntryType = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    PersonenIndexEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    LinkedDocumentId = table.Column<int>(type: "INTEGER", nullable: true),
                    LinkedDocumentType = table.Column<string>(type: "TEXT", nullable: true),
                    LinkedDocumentNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CounterAccount = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Quarter = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashEntries_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashEntries_CashBoxes_CashBoxId",
                        column: x => x.CashBoxId,
                        principalTable: "CashBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashEntries_PersonenIndexEntries_PersonenIndexEntryId",
                        column: x => x.PersonenIndexEntryId,
                        principalTable: "PersonenIndexEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntryNumber = table.Column<string>(type: "TEXT", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceReference = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalDebit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Quarter = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonenIndexEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalEntries_PersonenIndexEntries_PersonenIndexEntryId",
                        column: x => x.PersonenIndexEntryId,
                        principalTable: "PersonenIndexEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankReconciliations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BankStatementId = table.Column<int>(type: "INTEGER", nullable: false),
                    BankStatementLineId = table.Column<int>(type: "INTEGER", nullable: true),
                    DocumentType = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentNumber = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankReconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankReconciliations_BankStatementLines_BankStatementLineId",
                        column: x => x.BankStatementLineId,
                        principalTable: "BankStatementLines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankReconciliations_BankStatements_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "BankStatements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JournalEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountCode = table.Column<string>(type: "TEXT", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    IsDebit = table.Column<bool>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    LineDescription = table.Column<string>(type: "TEXT", nullable: true),
                    PersonenIndexEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    LineSequence = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_PersonenIndexEntries_PersonenIndexEntryId",
                        column: x => x.PersonenIndexEntryId,
                        principalTable: "PersonenIndexEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PersonenIndexEntryId",
                table: "Invoices",
                column: "PersonenIndexEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseInvoices_PersonenIndexEntryId",
                table: "ExpenseInvoices",
                column: "PersonenIndexEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_BankReconciliations_BankStatementId",
                table: "BankReconciliations",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_BankReconciliations_BankStatementLineId",
                table: "BankReconciliations",
                column: "BankStatementLineId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_BankStatementId",
                table: "BankStatementLines",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_BankAccountId",
                table: "BankStatements",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_BusinessId",
                table: "BankStatements",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBookDays_CashBoxId",
                table: "CashBookDays",
                column: "CashBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxes_BusinessId",
                table: "CashBoxes",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_BusinessId",
                table: "CashEntries",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_CashBoxId",
                table: "CashEntries",
                column: "CashBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_PersonenIndexEntryId",
                table: "CashEntries",
                column: "PersonenIndexEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_BusinessId",
                table: "JournalEntries",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PersonenIndexEntryId",
                table: "JournalEntries",
                column: "PersonenIndexEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_PersonenIndexEntryId",
                table: "JournalEntryLines",
                column: "PersonenIndexEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonenIndexEntries_BusinessId",
                table: "PersonenIndexEntries",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonenIndexEntries_ClientId",
                table: "PersonenIndexEntries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonenIndexEntries_CountryId",
                table: "PersonenIndexEntries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonenIndexEntries_SuggestedExpenseAccountId",
                table: "PersonenIndexEntries",
                column: "SuggestedExpenseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonenIndexEntries_SuggestedIncomeAccountId",
                table: "PersonenIndexEntries",
                column: "SuggestedIncomeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonenIndexEntries_SupplierId",
                table: "PersonenIndexEntries",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseInvoices_PersonenIndexEntries_PersonenIndexEntryId",
                table: "ExpenseInvoices",
                column: "PersonenIndexEntryId",
                principalTable: "PersonenIndexEntries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_PersonenIndexEntries_PersonenIndexEntryId",
                table: "Invoices",
                column: "PersonenIndexEntryId",
                principalTable: "PersonenIndexEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseInvoices_PersonenIndexEntries_PersonenIndexEntryId",
                table: "ExpenseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_PersonenIndexEntries_PersonenIndexEntryId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "BankReconciliations");

            migrationBuilder.DropTable(
                name: "CashBookDays");

            migrationBuilder.DropTable(
                name: "CashEntries");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "BankStatementLines");

            migrationBuilder.DropTable(
                name: "CashBoxes");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "BankStatements");

            migrationBuilder.DropTable(
                name: "PersonenIndexEntries");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_PersonenIndexEntryId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseInvoices_PersonenIndexEntryId",
                table: "ExpenseInvoices");

            migrationBuilder.DropColumn(
                name: "PersonenIndexEntryId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PersonenIndexEntryId",
                table: "ExpenseInvoices");
        }
    }
}

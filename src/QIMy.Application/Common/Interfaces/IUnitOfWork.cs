using QIMy.Core.Entities;

namespace QIMy.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern для управления транзакциями
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Reference Data Repositories
    IRepository<TaxRate> TaxRates { get; }
    IAccountRepository Accounts { get; }
    IRepository<Currency> Currencies { get; }
    IRepository<PaymentMethod> PaymentMethods { get; }
    IRepository<Unit> Units { get; }
    IRepository<Product> Products { get; }
    IRepository<BankAccount> BankAccounts { get; }
    IRepository<Business> Businesses { get; }
    IRepository<Discount> Discounts { get; }

    // AR Module Repositories
    IRepository<Client> Clients { get; }
    IRepository<ClientType> ClientTypes { get; }
    IRepository<ClientArea> ClientAreas { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceItem> InvoiceItems { get; }
    IRepository<InvoiceDiscount> InvoiceDiscounts { get; }

    // ER Module Repositories
    IRepository<Supplier> Suppliers { get; }
    IRepository<ExpenseInvoice> ExpenseInvoices { get; }
    IRepository<ExpenseInvoiceItem> ExpenseInvoiceItems { get; }

    // Other Repositories
    IRepository<Tax> Taxes { get; }
    IRepository<Payment> Payments { get; }

    // Personen Index (Справочник контрагентов)
    IRepository<PersonenIndexEntry> PersonenIndexEntries { get; }

    // Journal Entries (BUCHUNGSSCHRITTE)
    IRepository<JournalEntry> JournalEntries { get; }
    IRepository<JournalEntryLine> JournalEntryLines { get; }

    // Bank Statements (БАНК)
    IRepository<BankStatement> BankStatements { get; }
    IRepository<BankStatementLine> BankStatementLines { get; }
    IRepository<BankReconciliation> BankReconciliations { get; }

    // Cash Management (КАССА)
    IRepository<CashEntry> CashEntries { get; }
    IRepository<CashBox> CashBoxes { get; }
    IRepository<CashBookDay> CashBookDays { get; }

    /// <summary>
    /// Сохранить все изменения в базе данных
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

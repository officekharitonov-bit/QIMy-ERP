using QIMy.Application.Common.Interfaces;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Repositories;

/// <summary>
/// Реализация Unit of Work pattern для управления транзакциями
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Lazy initialization для репозиториев
    private IRepository<TaxRate>? _taxRates;
    private IAccountRepository? _accounts;
    private IRepository<Currency>? _currencies;
    private IRepository<PaymentMethod>? _paymentMethods;
    private IRepository<Unit>? _units;
    private IRepository<Product>? _products;
    private IRepository<BankAccount>? _bankAccounts;
    private IRepository<Business>? _businesses;
    private IRepository<Discount>? _discounts;

    private IRepository<Client>? _clients;
    private IRepository<ClientType>? _clientTypes;
    private IRepository<ClientArea>? _clientAreas;
    private IRepository<Invoice>? _invoices;
    private IRepository<InvoiceItem>? _invoiceItems;
    private IRepository<InvoiceDiscount>? _invoiceDiscounts;

    private IRepository<Supplier>? _suppliers;
    private IRepository<ExpenseInvoice>? _expenseInvoices;
    private IRepository<ExpenseInvoiceItem>? _expenseInvoiceItems;

    private IRepository<Tax>? _taxes;
    private IRepository<Payment>? _payments;

    private IRepository<PersonenIndexEntry>? _personenIndexEntries;
    private IRepository<JournalEntry>? _journalEntries;
    private IRepository<JournalEntryLine>? _journalEntryLines;
    private IRepository<BankStatement>? _bankStatements;
    private IRepository<BankStatementLine>? _bankStatementLines;
    private IRepository<BankReconciliation>? _bankReconciliations;
    private IRepository<CashEntry>? _cashEntries;
    private IRepository<CashBox>? _cashBoxes;
    private IRepository<CashBookDay>? _cashBookDays;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Reference Data Repositories
    public IRepository<TaxRate> TaxRates =>
        _taxRates ??= new Repository<TaxRate>(_context);

    public IAccountRepository Accounts =>
        _accounts ??= new AccountRepository(_context);

    public IRepository<Currency> Currencies =>
        _currencies ??= new Repository<Currency>(_context);

    public IRepository<PaymentMethod> PaymentMethods =>
        _paymentMethods ??= new Repository<PaymentMethod>(_context);

    public IRepository<Unit> Units =>
        _units ??= new Repository<Unit>(_context);

    public IRepository<Product> Products =>
        _products ??= new Repository<Product>(_context);

    public IRepository<BankAccount> BankAccounts =>
        _bankAccounts ??= new Repository<BankAccount>(_context);

    public IRepository<Business> Businesses =>
        _businesses ??= new Repository<Business>(_context);

    public IRepository<Discount> Discounts =>
        _discounts ??= new Repository<Discount>(_context);

    // AR Module Repositories
    public IRepository<Client> Clients =>
        _clients ??= new ClientRepository(_context);  // Специализированный репозиторий

    public IRepository<ClientType> ClientTypes =>
        _clientTypes ??= new Repository<ClientType>(_context);

    public IRepository<ClientArea> ClientAreas =>
        _clientAreas ??= new Repository<ClientArea>(_context);

    public IRepository<Invoice> Invoices =>
        _invoices ??= new InvoiceRepository(_context);  // Специализированный репозиторий

    public IRepository<InvoiceItem> InvoiceItems =>
        _invoiceItems ??= new Repository<InvoiceItem>(_context);

    public IRepository<InvoiceDiscount> InvoiceDiscounts =>
        _invoiceDiscounts ??= new Repository<InvoiceDiscount>(_context);

    // ER Module Repositories
    public IRepository<Supplier> Suppliers =>
        _suppliers ??= new Repository<Supplier>(_context);

    public IRepository<ExpenseInvoice> ExpenseInvoices =>
        _expenseInvoices ??= new Repository<ExpenseInvoice>(_context);

    public IRepository<ExpenseInvoiceItem> ExpenseInvoiceItems =>
        _expenseInvoiceItems ??= new Repository<ExpenseInvoiceItem>(_context);

    // Other Repositories
    public IRepository<Tax> Taxes =>
        _taxes ??= new Repository<Tax>(_context);

    public IRepository<Payment> Payments =>
        _payments ??= new Repository<Payment>(_context);

    // Personen Index (Справочник контрагентов)
    public IRepository<PersonenIndexEntry> PersonenIndexEntries =>
        _personenIndexEntries ??= new Repository<PersonenIndexEntry>(_context);

    // Journal Entries (BUCHUNGSSCHRITTE)
    public IRepository<JournalEntry> JournalEntries =>
        _journalEntries ??= new Repository<JournalEntry>(_context);

    public IRepository<JournalEntryLine> JournalEntryLines =>
        _journalEntryLines ??= new Repository<JournalEntryLine>(_context);

    // Bank Statements (БАНК)
    public IRepository<BankStatement> BankStatements =>
        _bankStatements ??= new Repository<BankStatement>(_context);

    public IRepository<BankStatementLine> BankStatementLines =>
        _bankStatementLines ??= new Repository<BankStatementLine>(_context);

    public IRepository<BankReconciliation> BankReconciliations =>
        _bankReconciliations ??= new Repository<BankReconciliation>(_context);

    // Cash Management (КАССА)
    public IRepository<CashEntry> CashEntries =>
        _cashEntries ??= new Repository<CashEntry>(_context);

    public IRepository<CashBox> CashBoxes =>
        _cashBoxes ??= new Repository<CashBox>(_context);

    public IRepository<CashBookDay> CashBookDays =>
        _cashBookDays ??= new Repository<CashBookDay>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

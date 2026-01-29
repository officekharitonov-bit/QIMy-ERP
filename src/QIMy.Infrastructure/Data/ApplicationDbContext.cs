using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;

namespace QIMy.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    private readonly ICurrentBusinessIdAccessor _businessIdAccessor;

    /// <summary>
    /// Current tenant (BusinessId) for this scope. Used by global query filters.
    /// </summary>
    public int? CurrentBusinessId => _businessIdAccessor.CurrentBusinessId;

    /// <summary>
    /// Emergency bypass for seeding/admin maintenance.
    /// </summary>
    public bool BypassTenantFilter => _businessIdAccessor.BypassTenantFilter;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentBusinessIdAccessor businessIdAccessor)
        : base(options)
    {
        _businessIdAccessor = businessIdAccessor;
    }

    // Main entities
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<UserBusiness> UserBusinesses => Set<UserBusiness>();

    /// <summary>
    /// Personen Index - центральный справочник контрагентов
    /// Это "телефонная книга" системы - единственный источник правды
    /// </summary>
    public DbSet<PersonenIndexEntry> PersonenIndexEntries => Set<PersonenIndexEntry>();

    // Invoices (AR - Ausgangsrechnungen)
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InvoiceDiscount> InvoiceDiscounts => Set<InvoiceDiscount>();

    // Quotes (AG - Angebote)
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();

    // Returns (ST - Storno/Credit Notes)
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();

    // Delivery Notes (LS - Lieferscheine)
    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();
    public DbSet<DeliveryNoteItem> DeliveryNoteItems => Set<DeliveryNoteItem>();

    // Expense Invoices (ER - Eingangsrechnungen)
    public DbSet<ExpenseInvoice> ExpenseInvoices => Set<ExpenseInvoice>();
    public DbSet<ExpenseInvoiceItem> ExpenseInvoiceItems => Set<ExpenseInvoiceItem>();

    // Journal Entries (BUCHUNGSSCHRITTE - Бухгалтерские проводки)
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    // Bank Statements (БАНК)
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();

    // Cash Management (КАССА)
    public DbSet<CashEntry> CashEntries => Set<CashEntry>();
    public DbSet<CashBox> CashBoxes => Set<CashBox>();
    public DbSet<CashBookDay> CashBookDays => Set<CashBookDay>();

    // Numbering
    public DbSet<NumberingConfig> NumberingConfigs => Set<NumberingConfig>();

    // Reference data
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<ClientArea> ClientAreas => Set<ClientArea>();
    public DbSet<ClientType> ClientTypes => Set<ClientType>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<VatRateChangeLog> VatRateChangeLogs => Set<VatRateChangeLog>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<EuCountryData> EuCountryData => Set<EuCountryData>();

    // AI Services
    public DbSet<AiProcessingLog> AiProcessingLogs => Set<AiProcessingLog>();
    public DbSet<AiSuggestion> AiSuggestions => Set<AiSuggestion>();
    public DbSet<AnomalyAlert> AnomalyAlerts => Set<AnomalyAlert>();
    public DbSet<AiConfiguration> AiConfigurations => Set<AiConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure AppUser relationship with Business
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Business)
            .WithMany()
            .HasForeignKey(u => u.BusinessId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Tax relationships
        modelBuilder.Entity<Tax>()
            .HasOne(t => t.TaxRate)
            .WithMany()
            .HasForeignKey(t => t.TaxRateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tax>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Taxes)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Account relationship with default TaxRate
        modelBuilder.Entity<Account>()
            .HasOne(a => a.DefaultTaxRate)
            .WithMany()
            .HasForeignKey(a => a.DefaultTaxRateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure AiConfiguration unique constraint
        modelBuilder.Entity<AiConfiguration>()
            .HasIndex(a => a.BusinessId)
            .IsUnique();

        // Configure decimal precision
        ConfigureDecimalPrecision(modelBuilder);

        // Multi-tenant enforcement
        ApplyBusinessQueryFilters(modelBuilder);
    }

    public override int SaveChanges()
    {
        EnforceBusinessIsolation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceBusinessIsolation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyBusinessQueryFilters(ModelBuilder modelBuilder)
    {
        var mustHaveBusinessType = typeof(IMustHaveBusiness);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType == null)
                continue;

            if (!mustHaveBusinessType.IsAssignableFrom(clrType))
                continue;

            // e => BypassTenantFilter || (CurrentBusinessId != null && e.BusinessId == CurrentBusinessId.Value)
            var parameter = Expression.Parameter(clrType, "e");
            var businessIdProperty = Expression.Property(parameter, nameof(IMustHaveBusiness.BusinessId));

            var contextConstant = Expression.Constant(this);
            var bypassProperty = Expression.Property(contextConstant, nameof(BypassTenantFilter));
            var currentBusinessIdProperty = Expression.Property(contextConstant, nameof(CurrentBusinessId));

            var hasTenant = Expression.NotEqual(
                currentBusinessIdProperty,
                Expression.Constant(null, typeof(int?)));

            var currentBusinessIdValue = Expression.Convert(currentBusinessIdProperty, typeof(int));
            var businessMatch = Expression.Equal(businessIdProperty, currentBusinessIdValue);
            var tenantPredicate = Expression.AndAlso(hasTenant, businessMatch);

            var body = Expression.OrElse(bypassProperty, tenantPredicate);
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }

        // Dependent entities without BusinessId must still be tenant-isolated via their required parent.
        modelBuilder.Entity<InvoiceItem>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.Invoice.BusinessId == CurrentBusinessId.Value));
        modelBuilder.Entity<InvoiceDiscount>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.Invoice.BusinessId == CurrentBusinessId.Value));
        modelBuilder.Entity<QuoteItem>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.Quote.BusinessId == CurrentBusinessId.Value));
        modelBuilder.Entity<ReturnItem>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.Return.BusinessId == CurrentBusinessId.Value));
        modelBuilder.Entity<DeliveryNoteItem>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.DeliveryNote.BusinessId == CurrentBusinessId.Value));
        modelBuilder.Entity<ExpenseInvoiceItem>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.ExpenseInvoice.BusinessId == CurrentBusinessId.Value));

        modelBuilder.Entity<JournalEntryLine>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.JournalEntry.BusinessId == CurrentBusinessId.Value));

        modelBuilder.Entity<CashBookDay>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.CashBox.BusinessId == CurrentBusinessId.Value));

        modelBuilder.Entity<BankStatementLine>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.BankStatement.BusinessId == CurrentBusinessId.Value));
        modelBuilder.Entity<BankReconciliation>()
            .HasQueryFilter(e => BypassTenantFilter || (CurrentBusinessId != null && e.BankStatement.BusinessId == CurrentBusinessId.Value));

        modelBuilder.Entity<AiProcessingLog>()
            .HasQueryFilter(e => BypassTenantFilter ||
                                (CurrentBusinessId != null &&
                                 ((e.Invoice != null && e.Invoice.BusinessId == CurrentBusinessId.Value) ||
                                  (e.ExpenseInvoice != null && e.ExpenseInvoice.BusinessId == CurrentBusinessId.Value))));
        modelBuilder.Entity<AiSuggestion>()
            .HasQueryFilter(e => BypassTenantFilter ||
                                (CurrentBusinessId != null &&
                                 ((e.Invoice != null && e.Invoice.BusinessId == CurrentBusinessId.Value) ||
                                  (e.ExpenseInvoice != null && e.ExpenseInvoice.BusinessId == CurrentBusinessId.Value))));
        modelBuilder.Entity<AnomalyAlert>()
            .HasQueryFilter(e => BypassTenantFilter ||
                                (CurrentBusinessId != null &&
                                 ((e.Invoice != null && e.Invoice.BusinessId == CurrentBusinessId.Value) ||
                                  (e.ExpenseInvoice != null && e.ExpenseInvoice.BusinessId == CurrentBusinessId.Value))));
    }

    private void EnforceBusinessIsolation()
    {
        if (BypassTenantFilter)
            return;

        var currentBusinessId = CurrentBusinessId;

        foreach (var entry in ChangeTracker.Entries<IMustHaveBusiness>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            if (!currentBusinessId.HasValue)
            {
                throw new InvalidOperationException(
                    $"BusinessId is required but no current business is set. Entity: {entry.Entity.GetType().Name}");
            }

            // Ensure the entity is bound to the current business.
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.BusinessId == 0)
                    entry.Entity.BusinessId = currentBusinessId.Value;
            }

            if (entry.Entity.BusinessId != currentBusinessId.Value)
            {
                throw new InvalidOperationException(
                    $"Cross-business write attempt detected. CurrentBusinessId={currentBusinessId.Value}, EntityBusinessId={entry.Entity.BusinessId}, Entity={entry.Entity.GetType().Name}");
            }

            // Prevent changing tenant ownership (except for soft deletes).
            if (entry.State == EntityState.Modified)
            {
                var businessIdProperty = entry.Property(nameof(IMustHaveBusiness.BusinessId));

                // Check if entity has IsDeleted property and if it's being set to true
                var isDeletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
                bool isBeingDeleted = isDeletedProperty != null &&
                                     isDeletedProperty.CurrentValue is bool deletedValue &&
                                     deletedValue == true;

                if (businessIdProperty.IsModified && !isBeingDeleted)
                {
                    throw new InvalidOperationException(
                        $"Changing BusinessId is not allowed. Entity: {entry.Entity.GetType().Name}");
                }
            }
        }
    }

    private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
    {
        // Invoice amounts
        modelBuilder.Entity<Invoice>()
            .Property(i => i.SubTotal).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.PaidAmount).HasPrecision(18, 2);

        // InvoiceItem amounts
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceItem>()
            .Property(i => i.Quantity).HasPrecision(12, 2);

        // TaxRate
        modelBuilder.Entity<TaxRate>()
            .Property(t => t.Rate).HasPrecision(5, 2);

        // Product price
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasPrecision(18, 2);

        // Currency exchange rate
        modelBuilder.Entity<Currency>()
            .Property(c => c.ExchangeRate).HasPrecision(18, 6);

        // Discount percentage
        modelBuilder.Entity<Discount>()
            .Property(d => d.Percentage).HasPrecision(5, 2);

        // ExpenseInvoiceItem
        modelBuilder.Entity<ExpenseInvoiceItem>()
            .Property(e => e.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<ExpenseInvoiceItem>()
            .Property(e => e.TaxRate).HasPrecision(5, 2);
        modelBuilder.Entity<ExpenseInvoiceItem>()
            .Property(e => e.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<ExpenseInvoiceItem>()
            .Property(e => e.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<ExpenseInvoiceItem>()
            .Property(e => e.Quantity).HasPrecision(12, 2);

        // Payment amount
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount).HasPrecision(18, 2);

        // JournalEntry amounts
        modelBuilder.Entity<JournalEntry>()
            .Property(j => j.TotalDebit).HasPrecision(18, 2);
        modelBuilder.Entity<JournalEntry>()
            .Property(j => j.TotalCredit).HasPrecision(18, 2);

        // JournalEntryLine amounts
        modelBuilder.Entity<JournalEntryLine>()
            .Property(j => j.Amount).HasPrecision(18, 2);

        // BankStatement amounts
        modelBuilder.Entity<BankStatement>()
            .Property(b => b.OpeningBalance).HasPrecision(18, 2);
        modelBuilder.Entity<BankStatement>()
            .Property(b => b.ClosingBalance).HasPrecision(18, 2);
        modelBuilder.Entity<BankStatement>()
            .Property(b => b.TotalDebits).HasPrecision(18, 2);
        modelBuilder.Entity<BankStatement>()
            .Property(b => b.TotalCredits).HasPrecision(18, 2);

        // BankStatementLine amounts
        modelBuilder.Entity<BankStatementLine>()
            .Property(b => b.Amount).HasPrecision(18, 2);

        // BankReconciliation amounts
        modelBuilder.Entity<BankReconciliation>()
            .Property(b => b.Amount).HasPrecision(18, 2);

        // CashEntry amounts
        modelBuilder.Entity<CashEntry>()
            .Property(c => c.Amount).HasPrecision(18, 2);

        // CashBox balance
        modelBuilder.Entity<CashBox>()
            .Property(c => c.CurrentBalance).HasPrecision(18, 2);

        // CashBookDay amounts
        modelBuilder.Entity<CashBookDay>()
            .Property(c => c.OpeningBalance).HasPrecision(18, 2);
        modelBuilder.Entity<CashBookDay>()
            .Property(c => c.TotalIncome).HasPrecision(18, 2);
        modelBuilder.Entity<CashBookDay>()
            .Property(c => c.TotalExpense).HasPrecision(18, 2);
        modelBuilder.Entity<CashBookDay>()
            .Property(c => c.ExpectedClosingBalance).HasPrecision(18, 2);
        modelBuilder.Entity<CashBookDay>()
            .Property(c => c.ActualClosingBalance).HasPrecision(18, 2);
        modelBuilder.Entity<CashBookDay>()
            .Property(c => c.Variance).HasPrecision(18, 2);

        // AI entities
        modelBuilder.Entity<AiProcessingLog>()
            .Property(a => a.ConfidenceScore).HasPrecision(5, 4);
        modelBuilder.Entity<AiProcessingLog>()
            .Property(a => a.Cost).HasPrecision(10, 4);

        modelBuilder.Entity<AiSuggestion>()
            .Property(a => a.Confidence).HasPrecision(5, 4);

        modelBuilder.Entity<AnomalyAlert>()
            .Property(a => a.Severity).HasPrecision(5, 4);

        modelBuilder.Entity<AiConfiguration>()
            .Property(a => a.AutoApprovalThreshold).HasPrecision(18, 2);
        modelBuilder.Entity<AiConfiguration>()
            .Property(a => a.MinConfidenceScore).HasPrecision(5, 4);
    }
}

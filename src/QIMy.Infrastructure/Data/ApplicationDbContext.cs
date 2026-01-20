using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Main entities
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Business> Businesses => Set<Business>();

    // Invoices (AR - Ausgangsrechnungen)
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InvoiceDiscount> InvoiceDiscounts => Set<InvoiceDiscount>();

    // Expense Invoices (ER - Eingangsrechnungen)
    public DbSet<ExpenseInvoice> ExpenseInvoices => Set<ExpenseInvoice>();
    public DbSet<ExpenseInvoiceItem> ExpenseInvoiceItems => Set<ExpenseInvoiceItem>();

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

        // Configure decimal precision
        ConfigureDecimalPrecision(modelBuilder);
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
    }
}

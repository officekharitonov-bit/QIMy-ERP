using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Items)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Client)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        // Ensure required fields
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.IsDeleted = false;

        // Auto-generate invoice number if not provided
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            // Use timestamp-based unique number to satisfy unique index and required constraint
            // Example: INV-20260123-153045-123
            var baseNumber = $"INV-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}";

            // In the unlikely event of collision, append a short guid suffix
            var exists = await _context.Invoices.AnyAsync(i => i.InvoiceNumber == baseNumber);
            invoice.InvoiceNumber = exists ? $"{baseNumber}-{Guid.NewGuid().ToString()[..4]}" : baseNumber;
        }

        // Assign default currency if missing
        if (invoice.CurrencyId == 0)
        {
            var defaultCurrencyId = await _context.Currencies
                .IgnoreQueryFilters()
                .Where(c => c.IsDefault && !c.IsDeleted)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (defaultCurrencyId == 0)
            {
                // Fallback to the first available currency
                defaultCurrencyId = await _context.Currencies
                    .IgnoreQueryFilters()
                    .Where(c => !c.IsDeleted)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();
            }

            if (defaultCurrencyId == 0)
            {
                // No currencies exist — throw a clear error
                throw new InvalidOperationException("No currencies found. Please seed currencies and set a default currency.");
            }

            invoice.CurrencyId = defaultCurrencyId;
        }

        // Ensure all items have proper audit fields
        if (invoice.Items != null && invoice.Items.Count > 0)
        {
            foreach (var item in invoice.Items)
            {
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                item.IsDeleted = false;
            }
            
            invoice.TaxAmount = invoice.Items.Sum(ii => ii.TaxAmount);
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        
        // Detach the invoice from context to prevent tracking conflicts
        _context.Entry(invoice).State = EntityState.Detached;
        if (invoice.Items != null)
        {
            foreach (var item in invoice.Items)
            {
                _context.Entry(item).State = EntityState.Detached;
            }
        }
        
        return invoice;
    }

    public async Task UpdateInvoiceAsync(Invoice invoice)
    {
        invoice.UpdatedAt = DateTime.UtcNow;
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInvoiceAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice != null)
        {
            invoice.IsDeleted = true;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllInvoicesAsync();

        searchTerm = searchTerm.ToLower();
        return await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Items)
            .Where(i => !i.IsDeleted &&
                (i.InvoiceNumber.ToLower().Contains(searchTerm) ||
                 (i.Client != null && i.Client.CompanyName.ToLower().Contains(searchTerm))))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(int clientId)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .Where(i => i.ClientId == clientId && !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAmountAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return 0;

        return invoice.Items?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;
    }
}

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
            .Include(i => i.Client)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.IsDeleted = false;
        
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
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

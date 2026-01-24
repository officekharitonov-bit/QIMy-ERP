using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class ReturnService : IReturnService
{
    private readonly ApplicationDbContext _context;

    public ReturnService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Return>> GetAllReturnsAsync()
    {
        return await _context.Returns
            .Include(r => r.Client)
            .Include(r => r.Currency)
            .Include(r => r.Business)
            .Include(r => r.OriginalInvoice)
            .Include(r => r.Items)
            .ThenInclude(ri => ri.Product)
            .Include(r => r.Items)
            .ThenInclude(ri => ri.Tax)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task<Return?> GetReturnByIdAsync(int id)
    {
        return await _context.Returns
            .Include(r => r.Client)
            .Include(r => r.Currency)
            .Include(r => r.Business)
            .Include(r => r.OriginalInvoice)
            .Include(r => r.Items)
            .ThenInclude(ri => ri.Product)
            .Include(r => r.Items)
            .ThenInclude(ri => ri.Tax)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<Return> CreateReturnAsync(Return returnDoc)
    {
        returnDoc.CreatedAt = DateTime.UtcNow;
        returnDoc.UpdatedAt = DateTime.UtcNow;
        returnDoc.IsDeleted = false;

        _context.Returns.Add(returnDoc);
        await _context.SaveChangesAsync();
        return returnDoc;
    }

    public async Task<Return> UpdateReturnAsync(Return returnDoc)
    {
        var existing = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnDoc.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Return with ID {returnDoc.Id} not found");

        existing.ReturnNumber = returnDoc.ReturnNumber;
        existing.ReturnDate = returnDoc.ReturnDate;
        existing.ClientId = returnDoc.ClientId;
        existing.OriginalInvoiceId = returnDoc.OriginalInvoiceId;
        existing.CurrencyId = returnDoc.CurrencyId;
        existing.BusinessId = returnDoc.BusinessId;
        existing.SubTotal = returnDoc.SubTotal;
        existing.TaxAmount = returnDoc.TaxAmount;
        existing.TotalAmount = returnDoc.TotalAmount;
        existing.Status = returnDoc.Status;
        existing.Notes = returnDoc.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.Returns.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteReturnAsync(int id)
    {
        var returnDoc = await _context.Returns.FirstOrDefaultAsync(r => r.Id == id);
        if (returnDoc != null)
        {
            returnDoc.IsDeleted = true;
            returnDoc.UpdatedAt = DateTime.UtcNow;
            _context.Returns.Update(returnDoc);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Return>> SearchReturnsAsync(string searchTerm)
    {
        return await _context.Returns
            .Include(r => r.Client)
            .Include(r => r.Currency)
            .Where(r => !r.IsDeleted &&
                (r.ReturnNumber.Contains(searchTerm) ||
                 (r.Client != null && r.Client.CompanyName != null && r.Client.CompanyName.Contains(searchTerm)) ||
                 (r.Client != null && r.Client.Email != null && r.Client.Email.Contains(searchTerm))))
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Return>> GetReturnsByClientAsync(int clientId)
    {
        return await _context.Returns
            .Include(r => r.Items)
            .Where(r => r.ClientId == clientId && !r.IsDeleted)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Return>> GetReturnsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Returns
            .Include(r => r.Client)
            .Where(r => r.ReturnDate >= startDate && r.ReturnDate <= endDate && !r.IsDeleted)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Return>> GetReturnsByInvoiceAsync(int invoiceId)
    {
        return await _context.Returns
            .Include(r => r.Items)
            .Where(r => r.OriginalInvoiceId == invoiceId && !r.IsDeleted)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task AddReturnItemAsync(int returnId, ReturnItem item)
    {
        var returnDoc = await _context.Returns.FirstOrDefaultAsync(r => r.Id == returnId);
        if (returnDoc == null)
            throw new KeyNotFoundException($"Return with ID {returnId} not found");

        item.ReturnId = returnId;
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.IsDeleted = false;

        _context.ReturnItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateReturnItemAsync(ReturnItem item)
    {
        var existing = await _context.ReturnItems.FirstOrDefaultAsync(ri => ri.Id == item.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Return Item with ID {item.Id} not found");

        existing.ProductId = item.ProductId;
        existing.Description = item.Description;
        existing.Quantity = item.Quantity;
        existing.UnitPrice = item.UnitPrice;
        existing.TaxId = item.TaxId;
        existing.TaxAmount = item.TaxAmount;
        existing.TotalAmount = item.TotalAmount;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.ReturnItems.Update(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReturnItemAsync(int itemId)
    {
        var item = await _context.ReturnItems.FirstOrDefaultAsync(ri => ri.Id == itemId);
        if (item != null)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.ReturnItems.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}

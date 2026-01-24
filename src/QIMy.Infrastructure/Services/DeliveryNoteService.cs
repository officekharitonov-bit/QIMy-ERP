using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class DeliveryNoteService : IDeliveryNoteService
{
    private readonly ApplicationDbContext _context;

    public DeliveryNoteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DeliveryNote>> GetAllDeliveryNotesAsync()
    {
        return await _context.DeliveryNotes
            .Include(d => d.Client)
            .Include(d => d.Business)
            .Include(d => d.Invoice)
            .Include(d => d.Items)
            .ThenInclude(di => di.Product)
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<DeliveryNote?> GetDeliveryNoteByIdAsync(int id)
    {
        return await _context.DeliveryNotes
            .Include(d => d.Client)
            .Include(d => d.Business)
            .Include(d => d.Invoice)
            .Include(d => d.Items)
            .ThenInclude(di => di.Product)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<DeliveryNote> CreateDeliveryNoteAsync(DeliveryNote deliveryNote)
    {
        deliveryNote.CreatedAt = DateTime.UtcNow;
        deliveryNote.UpdatedAt = DateTime.UtcNow;
        deliveryNote.IsDeleted = false;

        _context.DeliveryNotes.Add(deliveryNote);
        await _context.SaveChangesAsync();
        return deliveryNote;
    }

    public async Task<DeliveryNote> UpdateDeliveryNoteAsync(DeliveryNote deliveryNote)
    {
        var existing = await _context.DeliveryNotes.FirstOrDefaultAsync(d => d.Id == deliveryNote.Id);
        if (existing == null)
            throw new KeyNotFoundException($"DeliveryNote with ID {deliveryNote.Id} not found");

        existing.DeliveryNumber = deliveryNote.DeliveryNumber;
        existing.DeliveryDate = deliveryNote.DeliveryDate;
        existing.ClientId = deliveryNote.ClientId;
        existing.InvoiceId = deliveryNote.InvoiceId;
        existing.BusinessId = deliveryNote.BusinessId;
        existing.Status = deliveryNote.Status;
        existing.DeliveryAddress = deliveryNote.DeliveryAddress;
        existing.Notes = deliveryNote.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.DeliveryNotes.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteDeliveryNoteAsync(int id)
    {
        var deliveryNote = await _context.DeliveryNotes.FirstOrDefaultAsync(d => d.Id == id);
        if (deliveryNote != null)
        {
            deliveryNote.IsDeleted = true;
            deliveryNote.UpdatedAt = DateTime.UtcNow;
            _context.DeliveryNotes.Update(deliveryNote);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<DeliveryNote>> SearchDeliveryNotesAsync(string searchTerm)
    {
        return await _context.DeliveryNotes
            .Include(d => d.Client)
            .Where(d => !d.IsDeleted &&
                (d.DeliveryNumber.Contains(searchTerm) ||
                 d.Client.CompanyName.Contains(searchTerm) ||
                 d.Client.Email.Contains(searchTerm)))
            .OrderByDescending(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeliveryNote>> GetDeliveryNotesByClientAsync(int clientId)
    {
        return await _context.DeliveryNotes
            .Include(d => d.Items)
            .Where(d => d.ClientId == clientId && !d.IsDeleted)
            .OrderByDescending(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeliveryNote>> GetDeliveryNotesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.DeliveryNotes
            .Include(d => d.Client)
            .Where(d => d.DeliveryDate >= startDate && d.DeliveryDate <= endDate && !d.IsDeleted)
            .OrderByDescending(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeliveryNote>> GetDeliveryNotesByInvoiceAsync(int invoiceId)
    {
        return await _context.DeliveryNotes
            .Include(d => d.Items)
            .Where(d => d.InvoiceId == invoiceId && !d.IsDeleted)
            .OrderByDescending(d => d.DeliveryDate)
            .ToListAsync();
    }

    public async Task AddDeliveryNoteItemAsync(int deliveryNoteId, DeliveryNoteItem item)
    {
        var deliveryNote = await _context.DeliveryNotes.FirstOrDefaultAsync(d => d.Id == deliveryNoteId);
        if (deliveryNote == null)
            throw new KeyNotFoundException($"DeliveryNote with ID {deliveryNoteId} not found");

        item.DeliveryNoteId = deliveryNoteId;
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.IsDeleted = false;

        _context.DeliveryNoteItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDeliveryNoteItemAsync(DeliveryNoteItem item)
    {
        var existing = await _context.DeliveryNoteItems.FirstOrDefaultAsync(di => di.Id == item.Id);
        if (existing == null)
            throw new KeyNotFoundException($"DeliveryNote Item with ID {item.Id} not found");

        existing.ProductId = item.ProductId;
        existing.Description = item.Description;
        existing.Quantity = item.Quantity;
        existing.QuantityDelivered = item.QuantityDelivered;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.DeliveryNoteItems.Update(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDeliveryNoteItemAsync(int itemId)
    {
        var item = await _context.DeliveryNoteItems.FirstOrDefaultAsync(di => di.Id == itemId);
        if (item != null)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.DeliveryNoteItems.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}

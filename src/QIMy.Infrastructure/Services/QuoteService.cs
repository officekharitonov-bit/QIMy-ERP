using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class QuoteService : IQuoteService
{
    private readonly ApplicationDbContext _context;

    public QuoteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Quote>> GetAllQuotesAsync()
    {
        return await _context.Quotes
            .Include(q => q.Client)
            .Include(q => q.Currency)
            .Include(q => q.Business)
            .Include(q => q.Items)
            .ThenInclude(qi => qi.Product)
            .Include(q => q.Items)
            .ThenInclude(qi => qi.Tax)
            .Where(q => !q.IsDeleted)
            .OrderByDescending(q => q.QuoteDate)
            .ToListAsync();
    }

    public async Task<Quote?> GetQuoteByIdAsync(int id)
    {
        return await _context.Quotes
            .Include(q => q.Client)
            .Include(q => q.Currency)
            .Include(q => q.Business)
            .Include(q => q.Items)
            .ThenInclude(qi => qi.Product)
            .Include(q => q.Items)
            .ThenInclude(qi => qi.Tax)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
    }

    public async Task<Quote> CreateQuoteAsync(Quote quote)
    {
        quote.CreatedAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;
        quote.IsDeleted = false;

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();
        return quote;
    }

    public async Task<Quote> UpdateQuoteAsync(Quote quote)
    {
        var existing = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == quote.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Quote with ID {quote.Id} not found");

        existing.QuoteNumber = quote.QuoteNumber;
        existing.QuoteDate = quote.QuoteDate;
        existing.ValidUntil = quote.ValidUntil;
        existing.ClientId = quote.ClientId;
        existing.CurrencyId = quote.CurrencyId;
        existing.BusinessId = quote.BusinessId;
        existing.SubTotal = quote.SubTotal;
        existing.TaxAmount = quote.TaxAmount;
        existing.TotalAmount = quote.TotalAmount;
        existing.Status = quote.Status;
        existing.Notes = quote.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.Quotes.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteQuoteAsync(int id)
    {
        var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        if (quote != null)
        {
            quote.IsDeleted = true;
            quote.UpdatedAt = DateTime.UtcNow;
            _context.Quotes.Update(quote);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Quote>> SearchQuotesAsync(string searchTerm)
    {
        return await _context.Quotes
            .Include(q => q.Client)
            .Include(q => q.Currency)
            .Where(q => !q.IsDeleted &&
                (q.QuoteNumber.Contains(searchTerm) ||
                 q.Client.CompanyName.Contains(searchTerm) ||
                 q.Client.Email.Contains(searchTerm)))
            .OrderByDescending(q => q.QuoteDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quote>> GetQuotesByClientAsync(int clientId)
    {
        return await _context.Quotes
            .Include(q => q.Items)
            .Where(q => q.ClientId == clientId && !q.IsDeleted)
            .OrderByDescending(q => q.QuoteDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quote>> GetQuotesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Quotes
            .Include(q => q.Client)
            .Where(q => q.QuoteDate >= startDate && q.QuoteDate <= endDate && !q.IsDeleted)
            .OrderByDescending(q => q.QuoteDate)
            .ToListAsync();
    }

    public async Task AddQuoteItemAsync(int quoteId, QuoteItem item)
    {
        var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == quoteId);
        if (quote == null)
            throw new KeyNotFoundException($"Quote with ID {quoteId} not found");

        item.QuoteId = quoteId;
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.IsDeleted = false;

        _context.QuoteItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuoteItemAsync(QuoteItem item)
    {
        var existing = await _context.QuoteItems.FirstOrDefaultAsync(qi => qi.Id == item.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Quote Item with ID {item.Id} not found");

        existing.ProductId = item.ProductId;
        existing.Description = item.Description;
        existing.Quantity = item.Quantity;
        existing.UnitPrice = item.UnitPrice;
        existing.TaxId = item.TaxId;
        existing.TaxAmount = item.TaxAmount;
        existing.TotalAmount = item.TotalAmount;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.QuoteItems.Update(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuoteItemAsync(int itemId)
    {
        var item = await _context.QuoteItems.FirstOrDefaultAsync(qi => qi.Id == itemId);
        if (item != null)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            _context.QuoteItems.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}

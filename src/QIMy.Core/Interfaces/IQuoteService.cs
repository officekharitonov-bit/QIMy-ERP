using QIMy.Core.Entities;

namespace QIMy.Core.Interfaces;

public interface IQuoteService
{
    Task<IEnumerable<Quote>> GetAllQuotesAsync();
    Task<Quote?> GetQuoteByIdAsync(int id);
    Task<Quote> CreateQuoteAsync(Quote quote);
    Task<Quote> UpdateQuoteAsync(Quote quote);
    Task DeleteQuoteAsync(int id);
    Task<IEnumerable<Quote>> SearchQuotesAsync(string searchTerm);
    Task<IEnumerable<Quote>> GetQuotesByClientAsync(int clientId);
    Task<IEnumerable<Quote>> GetQuotesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddQuoteItemAsync(int quoteId, QuoteItem item);
    Task UpdateQuoteItemAsync(QuoteItem item);
    Task DeleteQuoteItemAsync(int itemId);
}

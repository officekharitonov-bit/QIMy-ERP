using QIMy.Core.Entities;

namespace QIMy.Core.Interfaces;

public interface IReturnService
{
    Task<IEnumerable<Return>> GetAllReturnsAsync();
    Task<Return?> GetReturnByIdAsync(int id);
    Task<Return> CreateReturnAsync(Return returnDoc);
    Task<Return> UpdateReturnAsync(Return returnDoc);
    Task DeleteReturnAsync(int id);
    Task<IEnumerable<Return>> SearchReturnsAsync(string searchTerm);
    Task<IEnumerable<Return>> GetReturnsByClientAsync(int clientId);
    Task<IEnumerable<Return>> GetReturnsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Return>> GetReturnsByInvoiceAsync(int invoiceId);
    Task AddReturnItemAsync(int returnId, ReturnItem item);
    Task UpdateReturnItemAsync(ReturnItem item);
    Task DeleteReturnItemAsync(int itemId);
}

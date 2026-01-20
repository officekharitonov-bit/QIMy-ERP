using QIMy.Core.Entities;

namespace QIMy.Core.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
    Task<Invoice?> GetInvoiceByIdAsync(int id);
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task DeleteInvoiceAsync(int id);
    Task<IEnumerable<Invoice>> SearchInvoicesAsync(string searchTerm);
    Task<IEnumerable<Invoice>> GetInvoicesByClientIdAsync(int clientId);
    Task<decimal> GetTotalAmountAsync(int invoiceId);
}

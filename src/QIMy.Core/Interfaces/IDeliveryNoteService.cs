using QIMy.Core.Entities;

namespace QIMy.Core.Interfaces;

public interface IDeliveryNoteService
{
    Task<IEnumerable<DeliveryNote>> GetAllDeliveryNotesAsync();
    Task<DeliveryNote?> GetDeliveryNoteByIdAsync(int id);
    Task<DeliveryNote> CreateDeliveryNoteAsync(DeliveryNote deliveryNote);
    Task<DeliveryNote> UpdateDeliveryNoteAsync(DeliveryNote deliveryNote);
    Task DeleteDeliveryNoteAsync(int id);
    Task<IEnumerable<DeliveryNote>> SearchDeliveryNotesAsync(string searchTerm);
    Task<IEnumerable<DeliveryNote>> GetDeliveryNotesByClientAsync(int clientId);
    Task<IEnumerable<DeliveryNote>> GetDeliveryNotesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<DeliveryNote>> GetDeliveryNotesByInvoiceAsync(int invoiceId);
    Task AddDeliveryNoteItemAsync(int deliveryNoteId, DeliveryNoteItem item);
    Task UpdateDeliveryNoteItemAsync(DeliveryNoteItem item);
    Task DeleteDeliveryNoteItemAsync(int itemId);
}

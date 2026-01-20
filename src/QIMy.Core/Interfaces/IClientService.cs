using QIMy.Core.Entities;
using QIMy.Core.Models.ExportImport;

namespace QIMy.Core.Interfaces;

public interface IClientService
{
    Task<IEnumerable<Client>> GetAllClientsAsync();
    Task<Client?> GetClientByIdAsync(int id);
    Task<Client> CreateClientAsync(Client client);
    Task<Client> UpdateClientAsync(Client client);
    Task DeleteClientAsync(int id);
    Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm);
    Task<Client?> GetClientByVatNumberAsync(string vatNumber);
    Task<int> GenerateNextClientCodeAsync(int? clientAreaId);

    // Export/Import
    Task<byte[]> ExportToCsvAsync();
    Task<(bool Success, string Message, int ImportedCount)> ImportFromCsvAsync(Stream csvStream, bool updateExisting = false);
}

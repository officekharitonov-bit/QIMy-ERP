using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Генерирует следующий ClientCode на основе ClientAreaId
    /// Inland(1): 200000-229999, EU(2): 230000-259999, Ausländisch(3): 260000-299999
    /// </summary>
    public async Task<int> GenerateNextClientCodeAsync(int? clientAreaId)
    {
        int baseCode = clientAreaId switch
        {
            1 => 200000, // Inländisch
            2 => 230000, // EU
            3 => 260000, // Ausländisch
            _ => 200000
        };

        int maxRange = baseCode + 29999;

        var maxCode = await _context.Clients
            .Where(c => c.ClientCode >= baseCode && c.ClientCode <= maxRange)
            .MaxAsync(c => (int?)c.ClientCode);

        return maxCode.HasValue ? maxCode.Value + 1 : baseCode;
    }

    public async Task<IEnumerable<Client>> GetAllClientsAsync()
    {
        return await _context.Clients
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        return await _context.Clients
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Client> CreateClientAsync(Client client)
    {
        // Автогенерация ClientCode
        if (!client.ClientCode.HasValue)
        {
            client.ClientCode = await GenerateNextClientCodeAsync(client.ClientAreaId);
        }

        {
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.VatNumber == client.VatNumber);

            if (existingClient != null)
            {
                throw new InvalidOperationException("Клиент с VAT номером {client.VatNumber} уже существует: {existingClient.CompanyName}");
            }
        }

        client.CreatedAt = DateTime.UtcNow;
        client.IsDeleted = false;

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return client;
    }

    public async Task<Client> UpdateClientAsync(Client client)
    {
        // Проверка уникальности VAT номера (кроме самого себя)
        if (!string.IsNullOrWhiteSpace(client.VatNumber))
        {
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.VatNumber == client.VatNumber && c.Id != client.Id);

            if (existingClient != null)
            {
                throw new InvalidOperationException("Клиент с VAT номером {client.VatNumber} уже существует: {existingClient.CompanyName}");
            }
        }

        client.UpdatedAt = DateTime.UtcNow;

        _context.Clients.Update(client);
        await _context.SaveChangesAsync();

        return client;
    }

    public async Task DeleteClientAsync(int id)
    {
        var client = await GetClientByIdAsync(id);
        if (client != null)
        {
            client.IsDeleted = true;
            client.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllClientsAsync();

        searchTerm = searchTerm.ToLower();

        return await _context.Clients
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .Where(c => !c.IsDeleted &&
                (c.CompanyName.ToLower().Contains(searchTerm) ||
                 (c.Email != null && c.Email.ToLower().Contains(searchTerm)) ||
                 (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(searchTerm)) ||
                 (c.VatNumber != null && c.VatNumber.ToLower().Contains(searchTerm)) ||
                 (c.TaxNumber != null && c.TaxNumber.ToLower().Contains(searchTerm))))
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<Client?> GetClientByVatNumberAsync(string vatNumber)
    {
        if (string.IsNullOrWhiteSpace(vatNumber))
            return null;

        return await _context.Clients
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.VatNumber == vatNumber);
    }

    public async Task<byte[]> ExportToCsvAsync()
    {
        var clients = await _context.Clients
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();

        var exportData = clients.Select(c => new QIMy.Core.Models.ExportImport.ClientExportModel
        {
            Freifeld_01 = string.Empty,
            Kto_Nr = c.ClientCode ?? 0,
            Nachname = c.CompanyName ?? string.Empty,
            Freifeld_06 = c.Country ?? string.Empty,
            Strasse = c.Address ?? string.Empty,
            Plz = c.PostalCode ?? string.Empty,
            Ort = c.City ?? string.Empty,
            WAE = "EUR",
            ZZiel = "30",
            SktoProz1 = string.Empty,
            SktoTage1 = string.Empty,
            UID_Nummer = c.VatNumber ?? string.Empty,
            Freifeld_11 = string.Empty,
            Lief_Vorschlag_Gegenkonto = string.Empty,
            Freifeld_04 = string.Empty,
            Freifeld_05 = string.Empty,
            Kunden_Vorschlag_Gegenkonto = string.Empty,
            Freifeld_02 = c.ClientTypeId?.ToString() ?? string.Empty,
            Freifeld_03 = c.ClientAreaId?.ToString() ?? string.Empty,
            Filiale = string.Empty,
            Land_Nr = string.Empty,
            Warenbeschreibung = string.Empty
        }).ToList();

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, System.Text.Encoding.UTF8);

        // Настройка CsvHelper для немецкого формата с точкой с запятой
        var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        using var csv = new CsvHelper.CsvWriter(writer, config);
        csv.Context.RegisterClassMap<QIMy.Infrastructure.Services.Mapping.ClientExportMap>();

        csv.WriteRecords(exportData);
        writer.Flush();

        return memoryStream.ToArray();
    }

    public async Task<(bool Success, string Message, int ImportedCount)> ImportFromCsvAsync(Stream csvStream, bool updateExisting = false)
    {
        try
        {
            // Копируем поток в память асинхронно для избежания "Synchronous reads are not supported"
            using var memoryStream = new MemoryStream();
            await csvStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Используем кодировку Windows-1252 для немецких символов
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var encoding = System.Text.Encoding.GetEncoding("Windows-1252");
            using var reader = new StreamReader(memoryStream, encoding, detectEncodingFromByteOrderMarks: true);

            // Настройка CsvHelper для немецкого формата с точкой с запятой
            var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim
            };

            using var csv = new CsvHelper.CsvReader(reader, config);
            csv.Context.RegisterClassMap<QIMy.Infrastructure.Services.Mapping.ClientExportMap>();

            var records = csv.GetRecords<QIMy.Core.Models.ExportImport.ClientExportModel>().ToList();

            int importedCount = 0;

            foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.Nachname))
                    continue;

                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.ClientCode == record.Kto_Nr && !c.IsDeleted);

                if (existingClient != null)
                {
                    if (updateExisting)
                    {
                        existingClient.CompanyName = record.Nachname;
                        existingClient.Address = record.Strasse ?? string.Empty;
                        existingClient.PostalCode = record.Plz;
                        existingClient.City = record.Ort;
                        existingClient.VatNumber = record.UID_Nummer;
                        existingClient.Country = record.Freifeld_06 ?? string.Empty;
                        existingClient.UpdatedAt = DateTime.UtcNow;

                        if (int.TryParse(record.Freifeld_02, out var clientTypeId))
                            existingClient.ClientTypeId = clientTypeId;

                        if (int.TryParse(record.Freifeld_03, out var clientAreaId))
                            existingClient.ClientAreaId = clientAreaId;

                        importedCount++;
                    }
                }
                else
                {
                    var newClient = new Client
                    {
                        ClientCode = record.Kto_Nr,
                        CompanyName = record.Nachname,
                        Address = record.Strasse ?? string.Empty,
                        PostalCode = record.Plz,
                        City = record.Ort,
                        VatNumber = record.UID_Nummer,
                        Country = record.Freifeld_06 ?? string.Empty,
                        ClientTypeId = int.TryParse(record.Freifeld_02, out var ct) ? ct : 1, // Default B2B
                        ClientAreaId = int.TryParse(record.Freifeld_03, out var ca) ? ca : 1, // Default Inländisch
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.Clients.Add(newClient);
                    importedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return (true, "Успешно импортировано: {importedCount} клиентов", importedCount);
        }
        catch (Exception ex)
        {
            return (false, "Ошибка импорта: {ex.Message}", 0);
        }
    }
}

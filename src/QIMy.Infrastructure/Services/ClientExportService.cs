using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public class ClientExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientExportService> _logger;

    public ClientExportService(
        ApplicationDbContext context,
        ILogger<ClientExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> ExportToCSVAsync()
    {
        var clients = await _context.Clients
            .Include(c => c.ClientType)
            .Include(c => c.ClientArea)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.ClientCode)
            .ToListAsync();

        var csv = new StringBuilder();
        
        // Header (same format as BMD export)
        csv.AppendLine("Freifeld 01;Kto-Nr;Nachname;Freifeld 06;Straße;Plz;Ort;WAE;ZZiel;SktoProz1;SktoTage1;UID-Nummer;Freifeld 11;Lief-Vorschlag Gegenkonto;Freifeld 04;Freifeld 05;Kunden-Vorschlag Gegenkonto;Freifeld 02;Freifeld 03;filiale;Land-Nr;Warenbeschreibung");

        foreach (var client in clients)
        {
            var countryCode = GetCountryCode(client.Country);
            var landNr = GetLandNumber(client.ClientArea?.Code);
            
            csv.AppendLine($"{countryCode};{client.ClientCode};{EscapeCSV(client.CompanyName)};;{EscapeCSV(client.Address)};{client.PostalCode};{EscapeCSV(client.City)};EUR;0;0;0;{client.VatNumber};;;;;;1;20;;{landNr};");
        }

        _logger.LogInformation($"Exported {clients.Count} clients to CSV");
        
        return Encoding.GetEncoding("windows-1252").GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportToExcelAsync()
    {
        // TODO: Implement Excel export using EPPlus or similar
        throw new NotImplementedException("Excel export coming soon");
    }

    private string EscapeCSV(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Escape semicolons and quotes
        if (value.Contains(';') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private string GetCountryCode(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return "AT";

        var countryMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Austria", "AT" },
            { "Österreich", "AT" },
            { "Germany", "DE" },
            { "Deutschland", "DE" },
            { "France", "FR" },
            { "Frankreich", "FR" },
            { "Belgium", "BE" },
            { "Belgien", "BE" },
            { "Finland", "FI" },
            { "Finnland", "FI" },
            { "Sweden", "SW" },
            { "Schweden", "SW" }
        };

        return countryMappings.TryGetValue(country, out var code) ? code : "AT";
    }

    private string GetLandNumber(string? clientAreaCode)
    {
        return clientAreaCode switch
        {
            "1" => "1",  // Inländisch (Austria)
            "2" => "2",  // EU
            "3" => "3",  // Ausländisch
            _ => "1"
        };
    }
}

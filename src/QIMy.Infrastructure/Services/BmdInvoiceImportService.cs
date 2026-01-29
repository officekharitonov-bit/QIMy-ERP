using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;
using System.Globalization;
using System.Text;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Service for importing invoices from BMD NTCS CSV format
/// </summary>
public class BmdInvoiceImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BmdInvoiceImportService> _logger;
    private readonly CultureInfo _germanCulture;

    public BmdInvoiceImportService(
        ApplicationDbContext context,
        ILogger<BmdInvoiceImportService> logger)
    {
        _context = context;
        _logger = logger;
        _germanCulture = CultureInfo.GetCultureInfo("de-DE");
    }

    /// <summary>
    /// Import invoices from BMD NTCS CSV format
    /// </summary>
    public async Task<BmdImportResult> ImportFromCsvAsync(Stream csvStream, int businessId)
    {
        var result = new BmdImportResult();

        try
        {
            // Enable async reads for the stream
            var streamWithAsync = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: false);
            using var reader = streamWithAsync;

            // Skip header line
            var header = await reader.ReadLineAsync();
            if (header == null)
            {
                result.Errors.Add("CSV file is empty");
                return result;
            }

            _logger.LogInformation("Starting BMD CSV import for BusinessId={BusinessId}", businessId);

            var lineNumber = 1;
            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var invoice = await ParseBmdCsvLine(line, businessId);
                    if (invoice != null)
                    {
                        result.ParsedInvoices.Add(invoice);
                        result.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Line {lineNumber}: {ex.Message}";
                    result.Errors.Add(error);
                    result.ErrorCount++;
                    _logger.LogWarning("Failed to parse line {LineNumber}: {Error}", lineNumber, ex.Message);
                }
            }

            _logger.LogInformation("Import completed: {Success} successful, {Errors} errors",
                result.SuccessCount, result.ErrorCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import BMD CSV");
            result.Errors.Add($"Import failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Parse a single BMD CSV line into an Invoice entity
    /// </summary>
    private async Task<Invoice?> ParseBmdCsvLine(string line, int businessId)
    {
        // Split by semicolon
        var fields = line.Split(';');

        // BMD NTCS can have 28 or 29 fields (sometimes extid appears twice)
        if (fields.Length < 27)
        {
            _logger.LogWarning("Line has {Count} fields, expected at least 27", fields.Length);
            return null;
        }

        // Extract fields (using safe indexing)
        string GetField(int index) => index < fields.Length ? fields[index].Trim() : string.Empty;

        var clientCode = GetField(1); // konto
        var revenueAccount = GetField(2); // gkonto
        var buchDatum = GetField(3); // buchdatum
        var belegDatum = GetField(4); // belegdatum
        var belegNr = GetField(5); // belegnr
        var betragStr = GetField(6); // betrag (net amount)
        var steuerStr = GetField(7); // steuer (tax amount)
        var text = GetField(8); // text
        var buchTyp = GetField(9); // buchtyp (1=AR, 2=ER)
        var buchSymbol = GetField(10); // buchsymbol (AR/ER)
        var prozentStr = GetField(12); // prozent
        var steuercodeStr = GetField(13); // steuercode
        var waehrung = GetField(17); // waehrung
        var uidnr = GetField(26); // uidnr (VAT number) - might be at index 27 if 29 fields

        // Only import AR invoices (buchtyp=1)
        if (buchTyp != "1")
        {
            _logger.LogDebug("Skipping non-AR record (buchtyp={BuchTyp})", buchTyp);
            return null;
        }

        // Parse dates (German format: dd.MM.yyyy)
        if (!DateTime.TryParseExact(belegDatum, "dd.MM.yyyy", _germanCulture, DateTimeStyles.None, out var invoiceDate))
        {
            throw new FormatException($"Invalid date format: {belegDatum}");
        }

        // Parse amounts (German format: comma as decimal separator)
        if (!decimal.TryParse(betragStr, NumberStyles.Number, _germanCulture, out var subTotal))
        {
            throw new FormatException($"Invalid amount format: {betragStr}");
        }

        if (!decimal.TryParse(steuerStr, NumberStyles.Number, _germanCulture, out var taxAmount))
        {
            taxAmount = 0; // Tax might be 0 for intra-EU supplies
        }

        var totalAmount = subTotal + taxAmount;

        // Parse steuercode
        if (!int.TryParse(steuercodeStr, out var steuercode))
        {
            steuercode = 1; // Default to standard VAT
        }

        // Find or create client by ClientCode
        var client = await FindOrCreateClientByCodeAsync(clientCode, uidnr, businessId);
        if (client == null)
        {
            throw new InvalidOperationException($"Failed to find or create client with code {clientCode}");
        }

        // Find currency
        var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == waehrung);
        if (currency == null)
        {
            currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == "EUR");
        }

        // Create invoice number from buchsymbol + belegnr
        var invoiceNumber = $"{buchSymbol}{belegNr}";

        // Check if invoice already exists
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && i.BusinessId == businessId);

        if (existingInvoice != null)
        {
            _logger.LogDebug("Invoice {InvoiceNumber} already exists, skipping", invoiceNumber);
            return null;
        }

        // Determine invoice type from steuercode
        var invoiceType = GetInvoiceTypeFromSteuercode(steuercode);

        // Create invoice
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            DueDate = invoiceDate.AddDays(30), // Default 30 days payment terms
            ClientId = client.Id,
            BusinessId = businessId,
            CurrencyId = currency?.Id ?? 1,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Sent, // Imported invoices are considered sent
            InvoiceType = invoiceType,
            Steuercode = steuercode,
            Konto = revenueAccount,
            Notes = $"Imported from BMD CSV: {text}"
        };

        return invoice;
    }

    /// <summary>
    /// Find client by ClientCode, or create a placeholder if not found
    /// </summary>
    private async Task<Client?> FindOrCreateClientByCodeAsync(string clientCodeStr, string vatNumber, int businessId)
    {
        // Try to parse client code
        if (!int.TryParse(clientCodeStr, out var clientCode))
        {
            _logger.LogWarning("Invalid client code: {ClientCode}", clientCodeStr);
            return null;
        }

        // Try to find existing client by ClientCode
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.ClientCode == clientCode && c.BusinessId == businessId);

        if (client != null)
        {
            return client;
        }

        // Try to find by VAT number
        if (!string.IsNullOrWhiteSpace(vatNumber))
        {
            client = await _context.Clients
                .FirstOrDefaultAsync(c => c.VatNumber == vatNumber && c.BusinessId == businessId);

            if (client != null)
            {
                // Update client code if found by VAT
                client.ClientCode = clientCode;
                await _context.SaveChangesAsync();
                return client;
            }
        }

        // Create placeholder client
        _logger.LogInformation("Creating placeholder client with code {ClientCode}", clientCode);

        var newClient = new Client
        {
            BusinessId = businessId,
            ClientCode = clientCode,
            CompanyName = $"Imported Client {clientCode}",
            VatNumber = vatNumber,
            PaymentTermsDays = 30
        };

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return newClient;
    }

    /// <summary>
    /// Map Steuercode to InvoiceType
    /// </summary>
    private InvoiceType GetInvoiceTypeFromSteuercode(int steuercode)
    {
        return steuercode switch
        {
            1 => InvoiceType.Domestic, // Standard 20% VAT
            51 => InvoiceType.Export, // Tax-free export
            77 => InvoiceType.IntraEUSale, // Intra-EU supply
            88 => InvoiceType.ReverseCharge, // Reverse charge
            62 => InvoiceType.SmallBusinessExemption, // Kleinunternehmer
            78 => InvoiceType.TriangularTransaction, // Dreiecksgeschäft
            _ => InvoiceType.Domestic // Default to domestic
        };
    }

    /// <summary>
    /// Save imported invoices to database
    /// </summary>
    public async Task<int> SaveImportedInvoicesAsync(List<Invoice> invoices)
    {
        if (!invoices.Any())
            return 0;

        _context.Invoices.AddRange(invoices);
        var count = await _context.SaveChangesAsync();

        _logger.LogInformation("Saved {Count} imported invoices", count);
        return count;
    }
}

/// <summary>
/// Result of BMD CSV import operation
/// </summary>
public class BmdImportResult
{
    public List<Invoice> ParsedInvoices { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public bool HasErrors => Errors.Any();
}

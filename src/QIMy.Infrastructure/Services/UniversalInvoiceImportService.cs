using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QIMy.Core.Entities;
using QIMy.Core.Enums;
using QIMy.Infrastructure.Data;
using System.Globalization;
using System.Text;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Universal service for importing invoices from various CSV formats (BMD NTCS, SevDesk, etc.)
/// </summary>
public class UniversalInvoiceImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UniversalInvoiceImportService> _logger;
    private readonly CultureInfo _germanCulture;
    private readonly CultureInfo _englishCulture;

    // Field mappings detected from CSV headers
    private Dictionary<string, int> _fieldMap = new();
    private string _detectedFormat = "Unknown";

    public UniversalInvoiceImportService(
        ApplicationDbContext context,
        ILogger<UniversalInvoiceImportService> logger)
    {
        _context = context;
        _logger = logger;
        _germanCulture = CultureInfo.GetCultureInfo("de-DE");
        _englishCulture = CultureInfo.GetCultureInfo("en-US");
    }

    public async Task<BmdImportResult> ImportFromCsvAsync(Stream csvStream, int businessId)
    {
        var result = new BmdImportResult();

        try
        {
            var streamReader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: false);
            using var reader = streamReader;

            // Read and analyze header to detect format
            var header = await reader.ReadLineAsync();
            if (header == null)
            {
                result.Errors.Add("CSV file is empty");
                return result;
            }

            // Detect format and build field map
            _detectedFormat = DetectFormatAndBuildFieldMap(header);
            _logger.LogInformation("✅ Detected format: {Format} with {Count} mapped fields", _detectedFormat, _fieldMap.Count);

            var lineNumber = 1;
            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var invoice = await ParseCsvLine(line, businessId, lineNumber);
                    if (invoice != null)
                    {
                        result.ParsedInvoices.Add(invoice);
                        result.SuccessCount++;
                        _logger.LogDebug("✅ Parsed invoice: {InvoiceNumber}", invoice.InvoiceNumber);
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Line {lineNumber}: {ex.Message}";
                    result.Errors.Add(error);
                    result.ErrorCount++;
                    _logger.LogWarning("❌ Failed to parse line {LineNumber}: {Error}", lineNumber, ex.Message);
                }
            }

            _logger.LogInformation("Import completed: {Success} successful, {Errors} errors (Format: {Format})",
                result.SuccessCount, result.ErrorCount, _detectedFormat);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import CSV");
            result.Errors.Add($"Import failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Detect CSV format and build field mapping from headers
    /// </summary>
    private string DetectFormatAndBuildFieldMap(string headerLine)
    {
        var headers = headerLine.ToLower().Split(';').Select(h => h.Trim()).ToArray();
        _fieldMap.Clear();

        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i];

            // Map common field names from different systems
            switch (header)
            {
                // Client/Account codes
                case "konto":
                case "account":
                case "debtor":
                case "customer_number":
                case "customer":
                case "kundennummer":
                    _fieldMap["client_code"] = i;
                    break;

                // Invoice number
                case "belegnr":
                case "invoice_number":
                case "rechnungsnummer":
                case "document_number":
                case "number":
                    _fieldMap["invoice_number"] = i;
                    break;

                // Dates
                case "belegdatum":
                case "rechnungsdatum":
                case "invoice_date":
                case "date":
                case "datum":
                    _fieldMap["invoice_date"] = i;
                    break;

                case "buchdatum":
                case "booking_date":
                    _fieldMap["booking_date"] = i;
                    break;

                // Amounts
                case "betrag":
                case "netto":
                case "net_amount":
                case "amount":
                case "subtotal":
                case "nettobetrag":
                    _fieldMap["net_amount"] = i;
                    break;

                case "steuer":
                case "tax":
                case "vat":
                case "mwst":
                case "tax_amount":
                    _fieldMap["tax_amount"] = i;
                    break;

                case "brutto":
                case "total":
                case "total_amount":
                case "gross":
                case "bruttobetrag":
                    _fieldMap["total_amount"] = i;
                    break;

                // Description/Text
                case "text":
                case "description":
                case "beschreibung":
                case "notes":
                case "memo":
                    _fieldMap["description"] = i;
                    break;

                // Tax code/rate
                case "steuercode":
                case "tax_code":
                case "vat_code":
                    _fieldMap["tax_code"] = i;
                    break;

                case "prozent":
                case "tax_rate":
                case "vat_rate":
                case "steuersatz":
                case "rate":
                    _fieldMap["tax_rate"] = i;
                    break;

                // VAT number
                case "uidnr":
                case "vat_number":
                case "uid":
                case "ustid":
                case "vat_id":
                    _fieldMap["vat_number"] = i;
                    break;

                // Currency
                case "waehrung":
                case "currency":
                case "währung":
                    _fieldMap["currency"] = i;
                    break;

                // Type indicators
                case "buchtyp":
                case "type":
                case "document_type":
                case "doc_type":
                    _fieldMap["doc_type"] = i;
                    break;

                case "buchsymbol":
                case "symbol":
                    _fieldMap["doc_symbol"] = i;
                    break;

                // Revenue/GL account
                case "gkonto":
                case "revenue_account":
                case "erlöskonto":
                case "gl_account":
                    _fieldMap["revenue_account"] = i;
                    break;
            }
        }

        // Detect format based on field combination
        if (_fieldMap.ContainsKey("buchtyp") && _fieldMap.ContainsKey("steuercode") && _fieldMap.ContainsKey("gkonto"))
        {
            return "BMD NTCS";
        }
        else if (_fieldMap.ContainsKey("customer_number") || headers.Contains("sevdesk"))
        {
            return "SevDesk";
        }
        else if (_fieldMap.ContainsKey("client_code") && _fieldMap.ContainsKey("invoice_number"))
        {
            return "Generic CSV";
        }

        return "Unknown Format";
    }

    /// <summary>
    /// Parse a single CSV line into an Invoice entity
    /// </summary>
    private async Task<Invoice?> ParseCsvLine(string line, int businessId, int lineNumber)
    {
        var fields = line.Split(';');

        // Helper to safely get field value
        string GetField(string fieldName) =>
            _fieldMap.ContainsKey(fieldName) && _fieldMap[fieldName] < fields.Length
                ? fields[_fieldMap[fieldName]].Trim()
                : string.Empty;

        // For BMD NTCS: Only import AR invoices (buchtyp=1)
        if (_detectedFormat == "BMD NTCS")
        {
            var buchTyp = GetField("doc_type");
            if (buchTyp != "1")
            {
                return null; // Skip non-AR records
            }
        }

        // Extract required fields
        var clientCodeStr = GetField("client_code");
        var invoiceNumber = GetField("invoice_number");
        var invoiceDateStr = GetField("invoice_date");
        var netAmountStr = GetField("net_amount");
        var taxAmountStr = GetField("tax_amount");
        var description = GetField("description");
        var taxCodeStr = GetField("tax_code");
        var vatNumber = GetField("vat_number");

        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            throw new FormatException("Invoice number is required");
        }

        // Parse date (try multiple formats)
        DateTime invoiceDate;
        if (!DateTime.TryParseExact(invoiceDateStr, "dd.MM.yyyy", _germanCulture, DateTimeStyles.None, out invoiceDate))
        {
            if (!DateTime.TryParseExact(invoiceDateStr, "yyyy-MM-dd", _englishCulture, DateTimeStyles.None, out invoiceDate))
            {
                if (!DateTime.TryParse(invoiceDateStr, out invoiceDate))
                {
                    throw new FormatException($"Invalid date format: {invoiceDateStr}");
                }
            }
        }

        // Parse amounts (try German and English formats)
        decimal netAmount = 0;
        if (!string.IsNullOrWhiteSpace(netAmountStr))
        {
            if (!decimal.TryParse(netAmountStr, NumberStyles.Any, _germanCulture, out netAmount))
            {
                decimal.TryParse(netAmountStr, NumberStyles.Any, _englishCulture, out netAmount);
            }
        }

        decimal taxAmount = 0;
        if (!string.IsNullOrWhiteSpace(taxAmountStr))
        {
            if (!decimal.TryParse(taxAmountStr, NumberStyles.Any, _germanCulture, out taxAmount))
            {
                decimal.TryParse(taxAmountStr, NumberStyles.Any, _englishCulture, out taxAmount);
            }
        }

        decimal totalAmount = netAmount + taxAmount;

        // Find or create client
        var client = await FindOrCreateClientAsync(clientCodeStr, vatNumber, businessId);
        if (client == null)
        {
            throw new Exception($"Failed to find or create client with code: {clientCodeStr}");
        }

        // Determine invoice type from tax code
        var invoiceType = DetermineInvoiceType(taxCodeStr);

        // Check for duplicate
        var exists = await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber && i.BusinessId == businessId && !i.IsDeleted);

        if (exists)
        {
            _logger.LogDebug("Skipping duplicate invoice: {InvoiceNumber}", invoiceNumber);
            return null;
        }

        // Get default currency (EUR) - ignore filters to ensure we get the global currency
        var currency = await _context.Currencies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Code == "EUR" && !c.IsDeleted);

        if (currency == null)
        {
            _logger.LogError("❌ EUR currency not found in database!");
            return null;
        }

        // Create invoice entity
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            ClientId = client.Id,
            CurrencyId = currency.Id,
            InvoiceType = invoiceType,
            SubTotal = netAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Sent,
            Notes = string.IsNullOrWhiteSpace(description) ? $"Imported from {_detectedFormat}" : description,
            BusinessId = businessId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        return invoice;
    }

    /// <summary>
    /// Find existing client or create placeholder
    /// </summary>
    private async Task<Client?> FindOrCreateClientAsync(string clientCode, string vatNumber, int businessId)
    {
        if (string.IsNullOrWhiteSpace(clientCode) && string.IsNullOrWhiteSpace(vatNumber))
        {
            return null;
        }

        // Try parse client code as integer
        int? clientCodeInt = null;
        if (!string.IsNullOrWhiteSpace(clientCode) && int.TryParse(clientCode, out var codeValue))
        {
            clientCodeInt = codeValue;
        }

        // Try find by client code
        if (clientCodeInt.HasValue)
        {
            var byCode = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientCode == clientCodeInt.Value && c.BusinessId == businessId && !c.IsDeleted);

            if (byCode != null)
                return byCode;
        }

        // Try find by VAT number
        if (!string.IsNullOrWhiteSpace(vatNumber))
        {
            var byVat = await _context.Clients
                .FirstOrDefaultAsync(c => c.VatNumber == vatNumber && c.BusinessId == businessId && !c.IsDeleted);

            if (byVat != null)
            {
                // Update client code if missing
                if (!byVat.ClientCode.HasValue && clientCodeInt.HasValue)
                {
                    byVat.ClientCode = clientCodeInt.Value;
                    _context.Clients.Update(byVat);
                }
                return byVat;
            }
        }

        // Create placeholder client
        var newClient = new Client
        {
            ClientCode = clientCodeInt,
            CompanyName = $"Imported Client {clientCode}",
            VatNumber = vatNumber,
            BusinessId = businessId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created placeholder client: {Code} / {VatNumber}", clientCode, vatNumber);

        return newClient;
    }

    /// <summary>
    /// Determine invoice type from tax code
    /// </summary>
    private InvoiceType DetermineInvoiceType(string taxCodeStr)
    {
        if (string.IsNullOrWhiteSpace(taxCodeStr) || !int.TryParse(taxCodeStr, out var taxCode))
        {
            return InvoiceType.Domestic;
        }

        // BMD NTCS tax codes
        return taxCode switch
        {
            1 => InvoiceType.Domestic, // 20% Standard
            51 => InvoiceType.Export, // Export
            77 => InvoiceType.IntraEUSale, // Intra-EU
            88 => InvoiceType.ReverseCharge, // Reverse Charge
            _ => InvoiceType.Domestic
        };
    }

    /// <summary>
    /// Save imported invoices to database
    /// </summary>
    public async Task<int> SaveImportedInvoicesAsync(List<Invoice> invoices)
    {
        if (!invoices.Any())
            return 0;

        try
        {
            _context.Invoices.AddRange(invoices);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Saved {Count} invoices to database", invoices.Count);

            return invoices.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving invoices: {Message}. Inner: {Inner}",
                ex.Message,
                ex.InnerException?.Message ?? "N/A");
            throw;
        }
    }
}

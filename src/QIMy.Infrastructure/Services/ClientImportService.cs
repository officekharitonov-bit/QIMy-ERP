using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QIMy.Core.DTOs;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

public enum CsvFormat
{
    QIMy,  // QIMy export format
    BMD    // BMD NTCS export format
}

public class ClientImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientImportService> _logger;
    private readonly IClientService _clientService;

    public ClientImportService(
        ApplicationDbContext context,
        ILogger<ClientImportService> logger,
        IClientService clientService)
    {
        _context = context;
        _logger = logger;
        _clientService = clientService;
    }

    public async Task<(List<string> Headers, Dictionary<string, string> SampleData)> AnalyzeCsvStructureAsync(Stream fileStream, bool hasHeaderRow = true, string encodingName = "utf-8")
    {
        // Reset stream position to start
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        // Register code page provider for Windows-1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        // Use specified encoding
        var encoding = GetEncodingByName(encodingName);

        using var reader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        
        // Read first line
        var firstLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(firstLine))
        {
            // Reset position for next read
            if (fileStream.CanSeek) fileStream.Position = 0;
            return (new List<string>(), new Dictionary<string, string>());
        }

        List<string> headers;
        Dictionary<string, string> sampleData = new();
        string? sampleLine;

        if (hasHeaderRow)
        {
            // First line is header
            headers = firstLine.Split(';')
                .Select(h => h.Trim())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();
            
            // Read second line for sample data
            sampleLine = await reader.ReadLineAsync();
        }
        else
        {
            // First line is data, generate column names
            var columns = firstLine.Split(';');
            headers = Enumerable.Range(1, columns.Length)
                .Select(i => $"Колонка {i}")
                .ToList();
            
            // Use first line as sample data
            sampleLine = firstLine;
        }

        // Populate sample data
        if (!string.IsNullOrWhiteSpace(sampleLine))
        {
            var values = sampleLine.Split(';');
            for (int i = 0; i < Math.Min(headers.Count, values.Length); i++)
            {
                var value = values[i].Trim();
                if (!string.IsNullOrEmpty(value) && value.Length <= 50) // Limit sample length
                {
                    sampleData[headers[i]] = value;
                }
            }
        }

        // Reset stream position for next read
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        return (headers, sampleData);
    }

    public async Task<List<ClientImportDto>> PreviewImportAsync(Stream fileStream, Dictionary<string, string>? columnMapping = null, string encodingName = "utf-8")
    {
        var clients = new List<ClientImportDto>();
        
        // Reset stream and detect encoding
        if (fileStream.CanSeek)
            fileStream.Position = 0;
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = GetEncodingByName(encodingName);
        
        using var reader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        
        // Read header to determine format
        var header = await reader.ReadLineAsync();
        
        CsvFormat format;
        if (columnMapping != null && columnMapping.Any())
        {
            format = CsvFormat.QIMy; // Use custom mapping
            _logger.LogInformation($"Using custom column mapping: {columnMapping.Count} mappings");
            _logger.LogInformation($"CSV Header: {header}");
            _logger.LogInformation($"Mappings: {string.Join(", ", columnMapping.Select(m => $"{m.Key}={m.Value}"))}");
        }
        else
        {
            format = DetermineFormat(header);
            _logger.LogInformation($"Detected CSV format: {format}");
        }
        
        int rowNumber = 2;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var dto = columnMapping != null 
                ? ParseCsvLineWithMapping(line, rowNumber, header, columnMapping)
                : ParseCsvLine(line, rowNumber, format);
            
            await ValidateClientAsync(dto);
            clients.Add(dto);
            
            rowNumber++;
        }

        return clients;
    }

    public async Task<ImportResult> ImportClientsAsync(Stream fileStream, bool skipErrors = true, Dictionary<string, string>? columnMapping = null, string encodingName = "utf-8", int? businessId = null)
    {
        var startTime = DateTime.UtcNow;
        var result = new ImportResult
        {
            ImportedAt = startTime
        };

        try
        {
            var clients = await PreviewImportAsync(fileStream, columnMapping, encodingName);
            result.TotalRows = clients.Count;

            // Load reference data once
            var clientAreas = await _context.ClientAreas.ToDictionaryAsync(ca => ca.Code);
            var clientTypes = await _context.ClientTypes.ToDictionaryAsync(ct => ct.Code);
            var accounts = await _context.Accounts.Include(a => a.DefaultTaxRate).ToDictionaryAsync(a => a.AccountNumber);
            var currencies = await _context.Currencies.ToDictionaryAsync(c => c.Code);

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                foreach (var dto in clients)
                {
                    if (!dto.IsValid)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = dto.RowNumber,
                            ClientCode = dto.ClientCode,
                            CompanyName = dto.CompanyName,
                            ErrorMessage = "Validation failed",
                            Details = dto.ValidationErrors.ToArray()
                        });

                        if (!skipErrors)
                        {
                            await transaction.RollbackAsync();
                            throw new InvalidOperationException($"Import stopped at row {dto.RowNumber} due to validation errors");
                        }
                        continue;
                    }

                    try
                    {
                        // Parse ClientCode to int
                        if (!int.TryParse(dto.ClientCode, out var clientCodeInt))
                        {
                            result.ErrorCount++;
                            result.Errors.Add(new ImportError
                            {
                                RowNumber = dto.RowNumber,
                                ClientCode = dto.ClientCode,
                                CompanyName = dto.CompanyName,
                                ErrorMessage = "Invalid ClientCode format",
                                Details = new[] { $"ClientCode '{dto.ClientCode}' must be a valid integer" }
                            });
                            if (!skipErrors)
                            {
                                await transaction.RollbackAsync();
                                throw new InvalidOperationException($"Invalid ClientCode at row {dto.RowNumber}");
                            }
                            continue;
                        }

                        // Check if client already exists
                        var existingClient = await _context.Clients
                            .FirstOrDefaultAsync(c => c.ClientCode == clientCodeInt);

                        if (existingClient != null)
                        {
                            result.SkippedCount++;
                            _logger.LogInformation($"Skipping existing client: {dto.ClientCode} - {dto.CompanyName}");
                            continue;
                        }

                        // Determine ClientArea based on country
                        var clientAreaCode = DetermineClientAreaCode(dto.CountryCode);
                        var clientArea = clientAreas.GetValueOrDefault(clientAreaCode);

                        // Determine ClientType (B2C for Barverkaufe, B2B for others)
                        var clientTypeCode = dto.CompanyName?.Contains("Barverkaufe", StringComparison.OrdinalIgnoreCase) == true ? "2" : "1";
                        var clientType = clientTypes.GetValueOrDefault(clientTypeCode);

                        var client = new Client
                        {
                            ClientCode = clientCodeInt,
                            CompanyName = dto.CompanyName ?? "Unknown",
                            ContactPerson = null,
                            Address = dto.Address,
                            City = dto.City,
                            PostalCode = dto.PostalCode,
                            Country = dto.Country,
                            Phone = null,
                            Email = null,
                            VatNumber = dto.VatNumber,
                            TaxNumber = null,
                            ClientTypeId = clientType?.Id,
                            ClientAreaId = clientArea?.Id,
                            BusinessId = businessId,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        };

                        _context.Clients.Add(client);
                        result.SuccessCount++;
                        
                        _logger.LogInformation($"Imported client: {client.ClientCode} - {client.CompanyName}");
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = dto.RowNumber,
                            ClientCode = dto.ClientCode,
                            CompanyName = dto.CompanyName,
                            ErrorMessage = ex.Message,
                            Details = new[] { ex.ToString() }
                        });

                        if (!skipErrors)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation($"Import completed: {result.SuccessCount} success, {result.ErrorCount} errors, {result.SkippedCount} skipped");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Import transaction failed");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed");
            throw;
        }

        result.Duration = DateTime.UtcNow - startTime;
        return result;
    }

    private CsvFormat DetermineFormat(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return CsvFormat.QIMy; // Default

        // BMD format starts with "Freifeld 01;Kto-Nr;"
        if (header.Contains("Kto-Nr") && header.Contains("Freifeld"))
            return CsvFormat.BMD;

        // QIMy format starts with "InternalClientId" or "CountryCode"
        if (header.Contains("InternalClientId") || header.Contains("CountryCode"))
            return CsvFormat.QIMy;

        // Default to QIMy
        return CsvFormat.QIMy;
    }

    private ClientImportDto ParseCsvLine(string line, int rowNumber, CsvFormat format)
    {
        if (format == CsvFormat.BMD)
            return ParseBmdCsvLine(line, rowNumber);
        else
            return ParseQimyCsvLine(line, rowNumber);
    }

    private ClientImportDto ParseQimyCsvLine(string line, int rowNumber)
    {
        var fields = line.Split(';');
        
        return new ClientImportDto
        {
            RowNumber = rowNumber,
            CountryCode = GetField(fields, 0),
            ClientCode = GetField(fields, 1),
            CompanyName = GetField(fields, 2),
            Country = GetField(fields, 3),
            Address = GetField(fields, 4),
            PostalCode = GetField(fields, 5),
            City = GetField(fields, 6),
            Currency = GetField(fields, 7),
            PaymentTerms = GetField(fields, 8),
            DiscountPercent = GetField(fields, 9),
            DiscountDays = GetField(fields, 10),
            VatNumber = GetField(fields, 11),
            AccountNumber = GetField(fields, 16)
        };
    }

    private ClientImportDto ParseBmdCsvLine(string line, int rowNumber)
    {
        var fields = line.Split(';');
        
        // BMD format columns:
        // 0: Freifeld 01 (empty)
        // 1: Kto-Nr (ClientCode)
        // 2: Nachname (CompanyName)
        // 3: Freifeld 06 (Address combined)
        // 4: Straße (Street - often empty)
        // 5: Plz (PostalCode)
        // 6: Ort (City)
        // 7: WAE (Currency)
        // 8: ZZiel (PaymentTerms)
        // 9: SktoProz1 (Discount %)
        // 10: SktoTage1 (Discount days)
        // 11: UID-Nummer (VAT Number)
        // 20: Land-Nr (Country code)
        
        var clientCode = GetField(fields, 1); // Kto-Nr
        var companyName = GetField(fields, 2); // Nachname
        var addressCombined = GetField(fields, 3); // Freifeld 06 - часто содержит адрес+город+страну
        var street = GetField(fields, 4);
        var postalCode = GetField(fields, 5);
        var city = GetField(fields, 6);
        var currency = GetField(fields, 7);
        var paymentTerms = GetField(fields, 8);
        var discountPercent = GetField(fields, 9);
        var discountDays = GetField(fields, 10);
        var vatNumber = GetField(fields, 11);
        var countryCode = GetField(fields, 20); // Land-Nr
        
        // Parse address from combined field if street is empty
        string? address = street;
        if (string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(addressCombined))
        {
            // Address might be in format: "Street\nPostalCode City\nCountry"
            var addressParts = addressCombined.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (addressParts.Length > 0)
                address = addressParts[0];
        }
        
        return new ClientImportDto
        {
            RowNumber = rowNumber,
            CountryCode = countryCode,
            ClientCode = clientCode,
            CompanyName = companyName,
            Country = ExtractCountryFromAddress(addressCombined),
            Address = address,
            PostalCode = postalCode,
            City = city,
            Currency = currency,
            PaymentTerms = paymentTerms,
            DiscountPercent = discountPercent,
            DiscountDays = discountDays,
            VatNumber = vatNumber,
            AccountNumber = null // BMD doesn't have this in client export
        };
    }
    
    private string? ExtractCountryFromAddress(string? addressCombined)
    {
        if (string.IsNullOrWhiteSpace(addressCombined))
            return null;
            
        // Try to extract country from combined address (last line usually)
        var lines = addressCombined.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            var lastLine = lines[^1].Trim();
            
            // Common country names/codes
            if (lastLine.Contains("Slovensko", StringComparison.OrdinalIgnoreCase))
                return "Slovakia";
            if (lastLine.Contains("PRAHA", StringComparison.OrdinalIgnoreCase))
                return "Czech Republic";
            if (lastLine.Contains("LV-", StringComparison.OrdinalIgnoreCase))
                return "Latvia";
                
            return lastLine;
        }
        
        return null;
    }

    private ClientImportDto ParseCsvLineWithMapping(string line, int rowNumber, string? header, Dictionary<string, string> columnMapping)
    {
        var headers = header?.Split(';').Select(h => h.Trim()).ToArray() ?? Array.Empty<string>();
        var values = line.Split(';');

        // Create index map: QIMy field -> CSV column index
        var indexMap = new Dictionary<string, int>();
        foreach (var mapping in columnMapping)
        {
            var qimyField = mapping.Key;
            var csvColumn = mapping.Value;
            var index = Array.IndexOf(headers, csvColumn);
            if (index >= 0)
            {
                indexMap[qimyField] = index;
            }
        }

        // Helper to get value by mapped field
        string? GetMappedField(string fieldName)
        {
            if (indexMap.TryGetValue(fieldName, out var index) && index < values.Length)
            {
                var value = values[index].Trim();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
            return null;
        }

        return new ClientImportDto
        {
            RowNumber = rowNumber,
            ClientCode = GetMappedField("ClientCode"),
            CompanyName = GetMappedField("CompanyName"),
            CountryCode = GetMappedField("Country") ?? GetMappedField("CountryCode"),
            Country = GetMappedField("Country") ?? GetMappedField("CountryCode"),
            Address = GetMappedField("Address"),
            City = GetMappedField("City"),
            PostalCode = GetMappedField("PostalCode"),
            VatNumber = GetMappedField("VatNumber"),
            Email = GetMappedField("Email"),
            Phone = GetMappedField("Phone"),
            ContactPerson = GetMappedField("ContactPerson"),
            AccountNumber = GetMappedField("AccountNumber"),
            PaymentTerms = GetMappedField("PaymentTerms") ?? GetMappedField("PaymentTermsCode"),
            DiscountPercent = GetMappedField("DiscountPercent"),
            DiscountDays = GetMappedField("DiscountDays"),
            Currency = GetMappedField("Currency"),
            TaxNumber = GetMappedField("TaxNumber"),
            Branch = GetMappedField("Branch"),
            CountryNumber = GetMappedField("CountryNumber"),
            Description = GetMappedField("Description") ?? GetMappedField("Notes"),
            ExternalAccountNumber = GetMappedField("ExternalAccountNumber")
        };
    }

    private string? GetField(string[] fields, int index)
    {
        if (index >= fields.Length) return null;
        var value = fields[index].Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private async Task ValidateClientAsync(ClientImportDto dto)
    {
        // Required fields
        if (string.IsNullOrWhiteSpace(dto.ClientCode))
            dto.ValidationErrors.Add("ClientCode is required");
        
        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            dto.ValidationErrors.Add("CompanyName is required");

        // Validate ClientCode format (should be numeric)
        if (!string.IsNullOrWhiteSpace(dto.ClientCode) && !int.TryParse(dto.ClientCode, out _))
            dto.ValidationErrors.Add($"ClientCode '{dto.ClientCode}' must be numeric");

        // Validate Currency
        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            var currencyExists = await _context.Currencies.AnyAsync(c => c.Code == dto.Currency);
            if (!currencyExists)
                dto.ValidationErrors.Add($"Currency '{dto.Currency}' not found in system");
        }

        // Validate Account
        if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
        {
            var accountExists = await _context.Accounts.AnyAsync(a => a.AccountNumber == dto.AccountNumber);
            if (!accountExists)
                dto.ValidationErrors.Add($"Account '{dto.AccountNumber}' not found in system");
        }

        // Validate VAT number format (basic)
        if (!string.IsNullOrWhiteSpace(dto.VatNumber) && dto.VatNumber.Length > 50)
            dto.ValidationErrors.Add("VatNumber is too long (max 50 characters)");
    }

    private string DetermineClientAreaCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return "1"; // Default to Inland

        // EU countries
        var euCountries = new[]
        {
            "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "ES", "FI",
            "FR", "GR", "HR", "HU", "IE", "IT", "LT", "LU", "LV", "MT",
            "NL", "PL", "PT", "RO", "SE", "SI", "SK"
        };

        if (countryCode == "AT")
            return "1"; // Inländisch (Austria)
        
        if (euCountries.Contains(countryCode.ToUpper()))
            return "2"; // EU
        
        return "3"; // Ausländisch (Third Countries)
    }

    private Encoding GetEncodingByName(string encodingName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        return encodingName.ToLower() switch
        {
            "utf-8" => Encoding.UTF8, // Standard UTF-8 with automatic BOM handling
            "utf-16" => Encoding.Unicode, // UTF-16 LE (Little Endian)
            "windows-1252" => Encoding.GetEncoding("Windows-1252"),
            "iso-8859-1" => Encoding.GetEncoding("ISO-8859-1"),
            _ => Encoding.UTF8
        };
    }

    private Encoding DetectEncoding(Stream stream)
    {
        if (!stream.CanSeek)
            return Encoding.UTF8;

        var position = stream.Position;
        
        // Read first bytes
        byte[] buffer = new byte[Math.Min(4096, (int)stream.Length)];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        stream.Position = position; // Reset position

        if (bytesRead == 0)
            return Encoding.UTF8;

        // Check for UTF-8 BOM (EF BB BF)
        if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            return new UTF8Encoding(true); // UTF-8 with BOM
        }

        // Check for UTF-16 LE BOM (FF FE)
        if (bytesRead >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
            return Encoding.Unicode;

        // Check for UTF-16 BE BOM (FE FF)
        if (bytesRead >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        // Try to decode as UTF-8 first (most common for modern files)
        try
        {
            var decoder = Encoding.UTF8.GetDecoder();
            char[] chars = new char[buffer.Length];
            int charCount = decoder.GetChars(buffer, 0, bytesRead, chars, 0, false);
            
            // If successful and no replacement characters, it's valid UTF-8
            if (charCount > 0 && !new string(chars, 0, charCount).Contains('\uFFFD'))
            {
                return new UTF8Encoding(false); // UTF-8 without BOM
            }
        }
        catch
        {
            // Not valid UTF-8, try Windows-1252
        }

        // Fall back to Windows-1252 for ANSI files
        try
        {
            return Encoding.GetEncoding("Windows-1252");
        }
        catch
        {
            return Encoding.UTF8; // Final fallback
        }
    }
}

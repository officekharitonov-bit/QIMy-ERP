using System.Data;
using System.IO;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Service for importing reference data from Personen Index Excel file
/// Imports: Countries, EU data (thresholds), Account codes
/// </summary>
public class PersonenIndexImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PersonenIndexImportService> _logger;

    public PersonenIndexImportService(
        ApplicationDbContext context,
        ILogger<PersonenIndexImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Import all data from Personen Index Excel file
    /// </summary>
    public async Task<PersonenIndexImportResult> ImportFromExcelAsync(string filePath)
    {
        var result = new PersonenIndexImportResult();

        try
        {
            _logger.LogInformation("Starting import from {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                result.Errors.Add($"File not found: {filePath}");
                return result;
            }

            // Create a temporary copy to avoid file locking issues
            var tempPath = Path.Combine(Path.GetTempPath(), $"personen_index_import_{Guid.NewGuid()}.xlsx");
            File.Copy(filePath, tempPath, overwrite: true);

            try
            {
                using var workbook = new XLWorkbook(tempPath);

                // Import Länder (Countries) from Sheet 6
                await ImportCountriesAsync(workbook, result);

                // Import EU-RATE data from Sheet 2
                await ImportEuCountryDataAsync(workbook, result);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Import completed: {Countries} countries, {EuData} EU data records",
                    result.CountriesImported, result.EuDataImported);

                return result;
            }
            finally
            {
                // Clean up temp file
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from Excel: {Message}", ex.Message);
            result.Errors.Add($"Import failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Import countries from Länder sheet (Sheet 6)
    /// </summary>
    private async Task ImportCountriesAsync(XLWorkbook workbook, PersonenIndexImportResult result)
    {
        try
        {
            var worksheet = workbook.Worksheet(6); // Länder sheet
            _logger.LogInformation("Found sheet: {Name}", worksheet.Name);

            var rows = worksheet.RowsUsed();
            _logger.LogInformation("Sheet 6 row range: {Range}", rows.Count());

            int rowNum = 0;
            foreach (var row in rows)
            {
                rowNum++;
                if (rowNum == 1) continue; // Skip header

                try
                {
                    var codeCell = row.Cell(1).GetValue<string>();
                    if (string.IsNullOrWhiteSpace(codeCell))
                        continue;

                    var code = codeCell.Trim();
                    var nameGerman = (row.Cell(2).GetValue<string>() ?? string.Empty).Trim();
                    var nameEnglish = (row.Cell(3).GetValue<string>() ?? string.Empty).Trim();
                    var countryNumberStr = (row.Cell(4).GetValue<string>() ?? "0").Trim();

                    if (string.IsNullOrWhiteSpace(code))
                        continue;

                    int.TryParse(countryNumberStr, out int countryNumber);

                    // Check if country already exists
                    var existing = await _context.Countries
                        .FirstOrDefaultAsync(c => c.Code == code);

                    if (existing == null)
                    {
                        var country = new Country
                        {
                            Code = code,
                            NameGerman = nameGerman,
                            NameEnglish = nameEnglish,
                            CountryNumber = countryNumber,
                            IsEuMember = IsEuMember(code),
                            CurrencyCode = GetCurrencyCode(code)
                        };

                        _context.Countries.Add(country);
                        result.CountriesImported++;
                    }
                    else
                    {
                        // Update existing
                        existing.NameGerman = nameGerman;
                        existing.NameEnglish = nameEnglish;
                        existing.CountryNumber = countryNumber;
                        existing.IsEuMember = IsEuMember(code);
                        existing.CurrencyCode = GetCurrencyCode(code);
                        result.CountriesUpdated++;
                    }
                }
                catch (Exception rowEx)
                {
                    _logger.LogWarning(rowEx, "Error processing row {RowNumber}", rowNum);
                    // Continue with next row
                }
            }

            await _context.SaveChangesAsync(); // Save to get IDs for next step
            _logger.LogInformation("Imported {Count} countries, Updated {Updated} countries", result.CountriesImported, result.CountriesUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing countries");
            result.Errors.Add($"Countries import error: {ex.Message}");
        }
    }

    /// <summary>
    /// Import EU country data from EU-RATE sheet (Sheet 2)
    /// Columns: Member States | Code | Standard Rate | LAND | Erwerbsschwelle | Lieferschwelle | WAE | Kto-Nr | ... | Land-NR
    /// </summary>
    private async Task ImportEuCountryDataAsync(XLWorkbook workbook, PersonenIndexImportResult result)
    {
        try
        {
            var worksheet = workbook.Worksheet(2); // EU-RATE sheet
            _logger.LogInformation("Found sheet: {Name}", worksheet.Name);

            var rows = worksheet.RowsUsed();
            _logger.LogInformation("Sheet 2 row range: {Range}", rows.Count());

            int rowNum = 0;
            foreach (var row in rows)
            {
                rowNum++;
                if (rowNum == 1) continue; // Skip header

                try
                {
                    var codeCell = row.Cell(2).GetValue<string>();
                    if (string.IsNullOrWhiteSpace(codeCell))
                        continue;

                    var code = codeCell.Trim(); // Code column

                    // Find corresponding country
                    var country = await _context.Countries
                        .Include(c => c.EuData)
                        .FirstOrDefaultAsync(c => c.Code == code);

                    if (country == null)
                    {
                        _logger.LogWarning("Country {Code} not found in Countries table", code);
                        continue;
                    }

                    // Parse thresholds (remove commas and parse)
                    var purchaseThresholdStr = (row.Cell(5).GetValue<string>() ?? "0").Replace(",", "").Trim();
                    var supplyThresholdStr = (row.Cell(6).GetValue<string>() ?? "0").Replace(",", "").Trim();
                    var accountNumber = (row.Cell(8).GetValue<string>() ?? string.Empty).Trim();
                    var countryNumberStr = (row.Cell(10).GetValue<string>() ?? "0").Trim();

                    decimal.TryParse(purchaseThresholdStr, out decimal purchaseThreshold);
                    decimal.TryParse(supplyThresholdStr, out decimal supplyThreshold);
                    int.TryParse(countryNumberStr, out int countryNumber);

                    if (country.EuData == null)
                    {
                        // Create new EU data
                        var euData = new EuCountryData
                        {
                            CountryId = country.Id,
                            CountryCode = code,
                            PurchaseThreshold = purchaseThreshold,
                            SupplyThreshold = supplyThreshold,
                            AccountNumber = accountNumber,
                            CountryNumber = countryNumber,
                            LastVerified = DateTime.UtcNow
                        };

                        _context.EuCountryData.Add(euData);
                        result.EuDataImported++;
                    }
                    else
                    {
                        // Update existing
                        country.EuData.PurchaseThreshold = purchaseThreshold;
                        country.EuData.SupplyThreshold = supplyThreshold;
                        country.EuData.AccountNumber = accountNumber;
                        country.EuData.CountryNumber = countryNumber;
                        country.EuData.LastVerified = DateTime.UtcNow;
                        result.EuDataUpdated++;
                    }
                }
                catch (Exception rowEx)
                {
                    _logger.LogWarning(rowEx, "Error processing EU data row {RowNumber}", rowNum);
                    // Continue with next row
                }
            }

            _logger.LogInformation("Imported {Count} EU data records, Updated {Updated}", result.EuDataImported, result.EuDataUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing EU country data");
            result.Errors.Add($"EU data import error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if country code belongs to EU member state
    /// </summary>
    private bool IsEuMember(string code)
    {
        var euCountries = new[]
        {
            "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
            "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
            "PL", "PT", "RO", "SK", "SI", "ES", "SE"
        };
        return euCountries.Contains(code.ToUpper());
    }

    /// <summary>
    /// Get currency code for country (simplified mapping)
    /// </summary>
    private string GetCurrencyCode(string countryCode)
    {
        return countryCode.ToUpper() switch
        {
            "AT" or "BE" or "CY" or "EE" or "FI" or "FR" or "DE" or "GR" or "IE" or
            "IT" or "LV" or "LT" or "LU" or "MT" or "NL" or "PT" or "SK" or "SI" or
            "ES" => "EUR",
            "BG" => "BGN",
            "HR" => "EUR", // Croatia adopted EUR in 2023
            "CZ" => "CZK",
            "DK" => "DKK",
            "HU" => "HUF",
            "PL" => "PLN",
            "RO" => "RON",
            "SE" => "SEK",
            "GB" or "UK" => "GBP",
            "CH" => "CHF",
            "NO" => "NOK",
            _ => "EUR"
        };
    }
}

/// <summary>
/// Result of Personen Index import operation
/// </summary>
public class PersonenIndexImportResult
{
    public int CountriesImported { get; set; }
    public int CountriesUpdated { get; set; }
    public int EuDataImported { get; set; }
    public int EuDataUpdated { get; set; }
    public List<string> Errors { get; set; } = new();

    public bool IsSuccess => Errors.Count == 0;

    public string Summary => $"Countries: {CountriesImported} imported, {CountriesUpdated} updated | " +
                            $"EU Data: {EuDataImported} imported, {EuDataUpdated} updated | " +
                            $"Errors: {Errors.Count}";
}

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QIMy.Core.DTOs;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

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

    public async Task<List<ClientImportDto>> PreviewImportAsync(Stream fileStream)
    {
        var clients = new List<ClientImportDto>();
        
        using var reader = new StreamReader(fileStream, Encoding.GetEncoding("windows-1252"));
        
        // Skip header
        await reader.ReadLineAsync();
        
        int rowNumber = 2;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var dto = ParseCsvLine(line, rowNumber);
            await ValidateClientAsync(dto);
            clients.Add(dto);
            
            rowNumber++;
        }

        return clients;
    }

    public async Task<ImportResult> ImportClientsAsync(Stream fileStream, bool skipErrors = true)
    {
        var startTime = DateTime.UtcNow;
        var result = new ImportResult
        {
            ImportedAt = startTime
        };

        try
        {
            var clients = await PreviewImportAsync(fileStream);
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

    private ClientImportDto ParseCsvLine(string line, int rowNumber)
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
}

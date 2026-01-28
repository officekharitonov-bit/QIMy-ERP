using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;
using System.Globalization;

namespace QIMy.Application.Suppliers.Commands.ImportSuppliers;

public class ImportSuppliersCommandHandler : IRequestHandler<ImportSuppliersCommand, Result<ImportSuppliersResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportSuppliersCommandHandler> _logger;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public ImportSuppliersCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ImportSuppliersCommandHandler> logger,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<Result<ImportSuppliersResult>> Handle(ImportSuppliersCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportSuppliersResult();
        var suppliers = new List<Supplier>();

        try
        {
            using var reader = new StreamReader(request.FileStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                Delimiter = ";"
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<SupplierCsvMap>();

            var records = csv.GetRecords<SupplierCsvRecord>().ToList();
            result.TotalRows = records.Count;

            int rowNumber = 1;
            foreach (var record in records)
            {
                rowNumber++;

                try
                {
                    // Parse Supplier Code
                    if (!int.TryParse(record.SupplierCode, out var supplierCode))
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = rowNumber,
                            CompanyName = record.CompanyName ?? "",
                            ErrorMessage = $"Invalid supplier code: '{record.SupplierCode}'"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // 🚫 FILTER: Skip client codes (200000-299999)
                    if (supplierCode >= 200000 && supplierCode <= 299999)
                    {
                        _logger.LogDebug("⏩ Строка {RowNumber}: Код {SupplierCode} - это клиент, пропускаем",
                            rowNumber, supplierCode);
                        continue; // Don't count as error, just skip
                    }

                    // Basic validation
                    if (string.IsNullOrWhiteSpace(record.CompanyName))
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = rowNumber,
                            CompanyName = record.CompanyName ?? "",
                            ErrorMessage = "Company name is required"
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check for duplicates
                    var duplicateResult = await _duplicateDetectionService.CheckSupplierDuplicateAsync(
                        record.CompanyName,
                        record.VatNumber,
                        null,
                        cancellationToken);

                    if (duplicateResult != null)
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = rowNumber,
                            CompanyName = record.CompanyName,
                            ErrorMessage = $"Duplicate: {duplicateResult.Message}"
                        });
                        result.DuplicateCount++;
                        continue;
                    }

                    var supplier = new Supplier
                    {
                        BusinessId = request.BusinessId,
                        SupplierCode = supplierCode, // NOW SET!
                        CompanyName = record.CompanyName,
                        ContactPerson = record.ContactPerson,
                        Email = record.Email,
                        Phone = record.Phone,
                        Address = record.Address,
                        City = record.City,
                        PostalCode = record.PostalCode,
                        Country = record.Country ?? "Österreich",
                        TaxNumber = record.TaxNumber,
                        VatNumber = record.VatNumber,
                        BankAccount = record.BankAccount,
                        CreatedAt = DateTime.UtcNow
                    };

                    suppliers.Add(supplier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing row {RowNumber}", rowNumber);
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = rowNumber,
                        CompanyName = record.CompanyName ?? "",
                        ErrorMessage = ex.Message
                    });
                    result.FailureCount++;
                }
            }

            // Bulk insert suppliers
            if (suppliers.Any())
            {
                foreach (var supplier in suppliers)
                {
                    await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                result.SuccessCount = suppliers.Count;
            }

            _logger.LogInformation(
                "Supplier import completed. Total: {Total}, Success: {Success}, Failed: {Failed}, Duplicates: {Duplicates}",
                result.TotalRows, result.SuccessCount, result.FailureCount, result.DuplicateCount);

            return Result<ImportSuppliersResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing suppliers from file {FileName}", request.FileName);
            return Result<ImportSuppliersResult>.Failure($"Error importing suppliers: {ex.Message}");
        }
    }
}

public class SupplierCsvRecord
{
    public string SupplierCode { get; set; } = string.Empty; // Kto-Nr (Column 1)
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? BankAccount { get; set; }
}

public sealed class SupplierCsvMap : ClassMap<SupplierCsvRecord>
{
    public SupplierCsvMap()
    {
        Map(m => m.SupplierCode).Index(1); // Column 1: Kto-Nr
        Map(m => m.CompanyName).Index(2); // Column 2: Nachname
        Map(m => m.ContactPerson).Name("ContactPerson", "Contact", "Kontakt").Optional();
        Map(m => m.Email).Name("Email", "E-Mail").Optional();
        Map(m => m.Phone).Name("Phone", "Telefon", "Tel").Optional();
        Map(m => m.Address).Name("Address", "Adresse").Optional();
        Map(m => m.City).Name("City", "Stadt", "Ort").Optional();
        Map(m => m.PostalCode).Name("PostalCode", "PLZ", "Zip").Optional();
        Map(m => m.Country).Index(3); // Column 3: Land
        Map(m => m.TaxNumber).Name("TaxNumber", "Steuernummer", "TaxNo").Optional();
        Map(m => m.VatNumber).Name("VatNumber", "UID", "VAT", "USt-IdNr").Optional();
        Map(m => m.BankAccount).Name("BankAccount", "IBAN", "Bank").Optional();
    }
}

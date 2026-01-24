using Microsoft.EntityFrameworkCore;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для обнаружения дубликатов перед созданием/обновлением.
/// </summary>
public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ApplicationDbContext _context;

    public DuplicateDetectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DuplicateDetectionResult?> CheckClientDuplicateAsync(
        string clientName,
        string? clientCode = null,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            return null;

        var clients = await _context.Clients
            .Where(c => (excludeId == null || c.Id != excludeId.Value) && !c.IsDeleted)
            .ToListAsync(ct);

        var duplicate = clients
            .FirstOrDefault(c => c.CompanyName.ToLowerInvariant() == clientName.ToLowerInvariant());

        if (duplicate != null)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "Client",
                DuplicateField = "CompanyName",
                DuplicateValue = clientName,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Клиент: {duplicate.CompanyName} (Код: {duplicate.ClientCode})",
                Severity = DuplicateSeverity.Warning,
                Message = $"Клиент с именем '{clientName}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = true
            };
        }

        if (!string.IsNullOrWhiteSpace(clientCode))
        {
            if (int.TryParse(clientCode, out var codeValue))
            {
                var codeDuplicate = await _context.Clients
                    .Where(c => c.ClientCode == codeValue &&
                                (excludeId == null || c.Id != excludeId.Value) &&
                                !c.IsDeleted)
                    .FirstOrDefaultAsync(ct);

                if (codeDuplicate != null)
                {
                    return new DuplicateDetectionResult
                    {
                        HasDuplicates = true,
                        EntityType = "Client",
                        DuplicateField = "ClientCode",
                        DuplicateValue = clientCode,
                        ExistingEntityId = codeDuplicate.Id,
                        ExistingEntityDetails = $"Клиент: {codeDuplicate.CompanyName} (Код: {codeDuplicate.ClientCode})",
                        Severity = DuplicateSeverity.Error,
                        Message = $"Код клиента '{clientCode}' уже используется",
                        DuplicateCount = 1,
                        RequireDoubleConfirmation = true
                    };
                }
            }
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckProductDuplicateAsync(
        string productName,
        string? productCode = null,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return null;

        var products = await _context.Products
            .Where(p => (excludeId == null || p.Id != excludeId.Value) && !p.IsDeleted)
            .ToListAsync(ct);

        var duplicate = products
            .FirstOrDefault(p => p.Name.ToLowerInvariant() == productName.ToLowerInvariant());

        if (duplicate != null)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "Product",
                DuplicateField = "Name",
                DuplicateValue = productName,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Товар: {duplicate.Name} (Цена: {duplicate.Price})",
                Severity = DuplicateSeverity.Warning,
                Message = $"Товар с названием '{productName}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = true
            };
        }

        if (!string.IsNullOrWhiteSpace(productCode))
        {
            var codeDuplicate = products
                .FirstOrDefault(p => (p.SKU ?? "").ToLowerInvariant() == productCode.ToLowerInvariant());

            if (codeDuplicate != null)
            {
                return new DuplicateDetectionResult
                {
                    HasDuplicates = true,
                    EntityType = "Product",
                    DuplicateField = "SKU",
                    DuplicateValue = productCode,
                    ExistingEntityId = codeDuplicate.Id,
                    ExistingEntityDetails = $"Товар: {codeDuplicate.Name}",
                    Severity = DuplicateSeverity.Error,
                    Message = $"Код товара '{productCode}' уже используется",
                    DuplicateCount = 1,
                    RequireDoubleConfirmation = true
                };
            }
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckSupplierDuplicateAsync(
        string supplierName,
        string? vatNumber = null,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
            return null;

        var suppliers = await _context.Suppliers
            .Where(s => (excludeId == null || s.Id != excludeId.Value) && !s.IsDeleted)
            .ToListAsync(ct);

        var duplicate = suppliers
            .FirstOrDefault(s => s.CompanyName.ToLowerInvariant() == supplierName.ToLowerInvariant());

        if (duplicate != null)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "Supplier",
                DuplicateField = "CompanyName",
                DuplicateValue = supplierName,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Поставщик: {duplicate.CompanyName} (VAT: {duplicate.VatNumber})",
                Severity = DuplicateSeverity.Warning,
                Message = $"Поставщик с названием '{supplierName}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = true
            };
        }

        if (!string.IsNullOrWhiteSpace(vatNumber))
        {
            var vatDuplicate = suppliers
                .FirstOrDefault(s => (s.VatNumber ?? "").ToLowerInvariant() == vatNumber.ToLowerInvariant());

            if (vatDuplicate != null)
            {
                return new DuplicateDetectionResult
                {
                    HasDuplicates = true,
                    EntityType = "Supplier",
                    DuplicateField = "VatNumber",
                    DuplicateValue = vatNumber,
                    ExistingEntityId = vatDuplicate.Id,
                    ExistingEntityDetails = $"Поставщик: {vatDuplicate.CompanyName}",
                    Severity = DuplicateSeverity.Error,
                    Message = $"Поставщик с VAT номером '{vatNumber}' уже существует",
                    DuplicateCount = 1,
                    RequireDoubleConfirmation = true
                };
            }
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckInvoiceDuplicateAsync(
        string invoiceNumber,
        int businessId,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return null;

        var invoices = await _context.Invoices
            .Include(i => i.Client)
            .Where(i => i.BusinessId == businessId &&
                        (excludeId == null || i.Id != excludeId.Value) &&
                        !i.IsDeleted)
            .ToListAsync(ct);

        var duplicate = invoices
            .FirstOrDefault(i => i.InvoiceNumber.ToLowerInvariant() == invoiceNumber.ToLowerInvariant());

        if (duplicate != null)
        {
            var clientName = duplicate.Client?.CompanyName ?? "Неизвестно";
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "Invoice",
                DuplicateField = "InvoiceNumber",
                DuplicateValue = invoiceNumber,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Счет #{duplicate.InvoiceNumber} от {duplicate.InvoiceDate:dd.MM.yyyy} Клиент: {clientName}",
                Severity = DuplicateSeverity.Error,
                Message = $"Счет с номером '{invoiceNumber}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = false
            };
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckExpenseInvoiceDuplicateAsync(
        string invoiceNumber,
        int businessId,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return null;

        var expenseInvoices = await _context.ExpenseInvoices
            .Include(e => e.Supplier)
            .Where(e => e.BusinessId == businessId &&
                        (excludeId == null || e.Id != excludeId.Value) &&
                        !e.IsDeleted)
            .ToListAsync(ct);

        var duplicate = expenseInvoices
            .FirstOrDefault(e => e.InvoiceNumber.ToLowerInvariant() == invoiceNumber.ToLowerInvariant());

        if (duplicate != null)
        {
            var supplierName = duplicate.Supplier?.CompanyName ?? "Неизвестно";
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "ExpenseInvoice",
                DuplicateField = "InvoiceNumber",
                DuplicateValue = invoiceNumber,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Входящий счет #{duplicate.InvoiceNumber} от {duplicate.InvoiceDate:dd.MM.yyyy} Поставщик: {supplierName}",
                Severity = DuplicateSeverity.Error,
                Message = $"Входящий счет с номером '{invoiceNumber}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = false
            };
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckCurrencyDuplicateAsync(
        string currencyCode,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return null;

        var duplicate = await _context.Currencies
            .Where(c => c.Code.ToLowerInvariant() == currencyCode.ToLowerInvariant() &&
                        (excludeId == null || c.Id != excludeId.Value) &&
                        !c.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (duplicate != null)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "Currency",
                DuplicateField = "Code",
                DuplicateValue = currencyCode,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Валюта: {duplicate.Name} ({duplicate.Code})",
                Severity = DuplicateSeverity.Error,
                Message = $"Валюта с кодом '{currencyCode}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = false
            };
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckTaxRateDuplicateAsync(
        decimal rate,
        string? country = null,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        var query = _context.TaxRates
            .Where(t => t.Rate == rate &&
                        (excludeId == null || t.Id != excludeId.Value) &&
                        !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(t => t.CountryCode == country);
        }

        var duplicate = await query.FirstOrDefaultAsync(ct);

        if (duplicate != null)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "TaxRate",
                DuplicateField = "Rate",
                DuplicateValue = rate.ToString(),
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Налоговая ставка: {duplicate.Rate}% для {duplicate.CountryCode}",
                Severity = DuplicateSeverity.Warning,
                Message = $"Налоговая ставка {rate}% уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = true
            };
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckCustomDuplicateAsync<T>(
        Func<IQueryable<T>, Task<bool>> checkFunc,
        string entityTypeName,
        string fieldName,
        string fieldValue,
        CancellationToken ct = default) where T : BaseEntity
    {
        var hasDuplicate = await checkFunc(_context.Set<T>());

        if (hasDuplicate)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = entityTypeName,
                DuplicateField = fieldName,
                DuplicateValue = fieldValue,
                Severity = DuplicateSeverity.Warning,
                Message = $"{entityTypeName} с {fieldName} '{fieldValue}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = true
            };
        }

        return null;
    }

    public async Task<DuplicateDetectionResult?> CheckPersonenIndexDuplicateAsync(
        string ktoNr,
        int? excludeId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ktoNr))
            return null;

        var duplicate = await _context.PersonenIndexEntries
            .Where(p => p.KtoNr.ToLowerInvariant() == ktoNr.ToLowerInvariant() &&
                        (excludeId == null || p.Id != excludeId.Value) &&
                        !p.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (duplicate != null)
        {
            return new DuplicateDetectionResult
            {
                HasDuplicates = true,
                EntityType = "PersonenIndexEntry",
                DuplicateField = "KtoNr",
                DuplicateValue = ktoNr,
                ExistingEntityId = duplicate.Id,
                ExistingEntityDetails = $"Контрагент: {duplicate.CompanyName} (KtoNr: {duplicate.KtoNr})",
                Severity = DuplicateSeverity.Error,
                Message = $"Контрагент с номером счета (KtoNr) '{ktoNr}' уже существует",
                DuplicateCount = 1,
                RequireDoubleConfirmation = false
            };
        }

        return null;
    }

    public Task<string> GetDuplicateDetailsAsync(
        string entityType,
        int entityId,
        CancellationToken ct = default)
    {
        return Task.FromResult(entityType.ToLower() switch
        {
            "client" => GetClientDetails(entityId),
            "product" => GetProductDetails(entityId),
            "supplier" => GetSupplierDetails(entityId),
            "invoice" => GetInvoiceDetails(entityId),
            "expenseinvoice" => GetExpenseInvoiceDetails(entityId),
            "currency" => GetCurrencyDetails(entityId),
            "taxrate" => GetTaxRateDetails(entityId),
            "personenindexentry" => GetPersonenIndexDetails(entityId),
            _ => "Неизвестная сущность"
        });
    }

    private string GetClientDetails(int clientId)
    {
        var client = _context.Clients.FirstOrDefault(c => c.Id == clientId);
        return client != null
            ? $"Клиент: {client.CompanyName} (Код: {client.ClientCode}, Email: {client.Email})"
            : "Клиент не найден";
    }

    private string GetProductDetails(int productId)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == productId);
        return product != null
            ? $"Товар: {product.Name} (SKU: {product.SKU}, Цена: {product.Price})"
            : "Товар не найден";
    }

    private string GetSupplierDetails(int supplierId)
    {
        var supplier = _context.Suppliers.FirstOrDefault(s => s.Id == supplierId);
        return supplier != null
            ? $"Поставщик: {supplier.CompanyName} (VAT: {supplier.VatNumber})"
            : "Поставщик не найден";
    }

    private string GetInvoiceDetails(int invoiceId)
    {
        var invoice = _context.Invoices
            .Include(i => i.Client)
            .FirstOrDefault(i => i.Id == invoiceId);
        return invoice != null
            ? $"Счет #{invoice.InvoiceNumber} от {invoice.InvoiceDate:dd.MM.yyyy} (Клиент: {invoice.Client?.CompanyName}, Сумма: {invoice.TotalAmount})"
            : "Счет не найден";
    }

    private string GetExpenseInvoiceDetails(int invoiceId)
    {
        var invoice = _context.ExpenseInvoices
            .Include(e => e.Supplier)
            .FirstOrDefault(e => e.Id == invoiceId);
        return invoice != null
            ? $"Входящий счет #{invoice.InvoiceNumber} от {invoice.InvoiceDate:dd.MM.yyyy} (Поставщик: {invoice.Supplier?.CompanyName}, Сумма: {invoice.TotalAmount})"
            : "Входящий счет не найден";
    }

    private string GetCurrencyDetails(int currencyId)
    {
        var currency = _context.Currencies.FirstOrDefault(c => c.Id == currencyId);
        return currency != null
            ? $"Валюта: {currency.Name} ({currency.Code})"
            : "Валюта не найдена";
    }

    private string GetTaxRateDetails(int taxRateId)
    {
        var taxRate = _context.TaxRates.FirstOrDefault(t => t.Id == taxRateId);
        return taxRate != null
            ? $"Налоговая ставка: {taxRate.Rate}% для {taxRate.CountryCode}"
            : "Налоговая ставка не найдена";
    }

    private string GetPersonenIndexDetails(int indexId)
    {
        var entry = _context.PersonenIndexEntries.FirstOrDefault(p => p.Id == indexId);
        return entry != null
            ? $"Контрагент: {entry.CompanyName} (KtoNr: {entry.KtoNr}, Страна: {entry.CountryCode})"
            : "Контрагент не найден";
    }
}

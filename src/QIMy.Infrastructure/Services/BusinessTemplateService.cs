using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Сервис для копирования справочников из шаблонного бизнеса при создании нового
/// </summary>
public class BusinessTemplateService
{
    private readonly ApplicationDbContext _context;

    public BusinessTemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Копирует все базовые справочники из исходного бизнеса в новый
    /// </summary>
    /// <param name="sourceBusinessId">ID бизнеса-шаблона (например, "AAA")</param>
    /// <param name="targetBusinessId">ID нового бизнеса</param>
    public async Task CopyReferenceDataAsync(int sourceBusinessId, int targetBusinessId)
    {
        // Копируем ClientAreas
        var sourceClientAreas = await _context.ClientAreas
            .IgnoreQueryFilters()
            .Where(ca => ca.BusinessId == sourceBusinessId)
            .ToListAsync();

        foreach (var source in sourceClientAreas)
        {
            _context.ClientAreas.Add(new ClientArea
            {
                Name = source.Name,
                Code = source.Code,
                Description = source.Description,
                BusinessId = targetBusinessId
            });
        }

        // Копируем ClientTypes
        var sourceClientTypes = await _context.ClientTypes
            .IgnoreQueryFilters()
            .Where(ct => ct.BusinessId == sourceBusinessId)
            .ToListAsync();

        foreach (var source in sourceClientTypes)
        {
            _context.ClientTypes.Add(new ClientType
            {
                Name = source.Name,
                Code = source.Code,
                Description = source.Description,
                BusinessId = targetBusinessId
            });
        }

        // Копируем TaxRates
        var sourceTaxRates = await _context.TaxRates
            .IgnoreQueryFilters()
            .Where(tr => tr.BusinessId == sourceBusinessId)
            .ToListAsync();

        foreach (var source in sourceTaxRates)
        {
            _context.TaxRates.Add(new TaxRate
            {
                CountryCode = source.CountryCode,
                CountryName = source.CountryName,
                Name = source.Name,
                Rate = source.Rate,
                RateType = source.RateType,
                IsDefault = source.IsDefault,
                EffectiveFrom = source.EffectiveFrom,
                EffectiveUntil = source.EffectiveUntil,
                Source = source.Source,
                Notes = source.Notes,
                BusinessId = targetBusinessId
            });
        }

        // Копируем Currencies
        var sourceCurrencies = await _context.Currencies
            .IgnoreQueryFilters()
            .Where(c => c.BusinessId == sourceBusinessId)
            .ToListAsync();

        foreach (var source in sourceCurrencies)
        {
            _context.Currencies.Add(new Currency
            {
                Code = source.Code,
                Name = source.Name,
                Symbol = source.Symbol,
                ExchangeRate = source.ExchangeRate,
                IsDefault = source.IsDefault,
                BusinessId = targetBusinessId
            });
        }

        // Копируем PaymentMethods
        var sourcePaymentMethods = await _context.PaymentMethods
            .IgnoreQueryFilters()
            .Where(pm => pm.BusinessId == sourceBusinessId)
            .ToListAsync();

        foreach (var source in sourcePaymentMethods)
        {
            _context.PaymentMethods.Add(new PaymentMethod
            {
                Name = source.Name,
                IsDefault = source.IsDefault,
                BusinessId = targetBusinessId
            });
        }

        // Копируем Units
        var sourceUnits = await _context.Units
            .IgnoreQueryFilters()
            .Where(u => u.BusinessId == sourceBusinessId)
            .ToListAsync();

        foreach (var source in sourceUnits)
        {
            _context.Units.Add(new Unit
            {
                Name = source.Name,
                ShortName = source.ShortName,
                IsDefault = source.IsDefault,
                BusinessId = targetBusinessId
            });
        }

        await _context.SaveChangesAsync();

        // Теперь копируем Accounts (нужно после TaxRates и ClientAreas)
        var sourceAccounts = await _context.Accounts
            .IgnoreQueryFilters()
            .Include(a => a.ClientArea)
            .Include(a => a.DefaultTaxRate)
            .Where(a => a.BusinessId == sourceBusinessId)
            .ToListAsync();

        var clientAreaMap = new Dictionary<string, int>();
        var newClientAreas = await _context.ClientAreas
            .IgnoreQueryFilters()
            .Where(ca => ca.BusinessId == targetBusinessId)
            .ToListAsync();
        foreach (var ca in newClientAreas)
        {
            if (ca.Code != null)
                clientAreaMap[ca.Code] = ca.Id;
        }

        var taxRateMap = new Dictionary<decimal, int>();
        var newTaxRates = await _context.TaxRates
            .IgnoreQueryFilters()
            .Where(tr => tr.BusinessId == targetBusinessId)
            .ToListAsync();
        foreach (var tr in newTaxRates)
        {
            taxRateMap[tr.Rate] = tr.Id;
        }

        foreach (var source in sourceAccounts)
        {
            var newClientAreaId = source.ClientArea?.Code != null && clientAreaMap.ContainsKey(source.ClientArea.Code)
                ? clientAreaMap[source.ClientArea.Code]
                : source.ClientAreaId;

            var newTaxRateId = source.DefaultTaxRate != null && taxRateMap.ContainsKey(source.DefaultTaxRate.Rate)
                ? (int?)taxRateMap[source.DefaultTaxRate.Rate]
                : source.DefaultTaxRateId;

            _context.Accounts.Add(new Account
            {
                AccountNumber = source.AccountNumber,
                Name = source.Name,
                AccountCode = source.AccountCode,
                ClientAreaId = newClientAreaId,
                DefaultTaxRateId = newTaxRateId,
                IsForServices = source.IsForServices,
                BusinessId = targetBusinessId
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Получает ID шаблонного бизнеса (первый созданный бизнес или бизнес с именем "Шаблон")
    /// </summary>
    public async Task<int?> GetTemplateBusinessIdAsync()
    {
        // Ищем бизнес "Шаблон" или "Template" или первый по дате создания
        var templateBusiness = await _context.Businesses
            .IgnoreQueryFilters()
            .Where(b => b.Name == "Шаблон" || b.Name == "Template" || b.Name == "AAA")
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (templateBusiness != null)
            return templateBusiness.Id;

        // Если не найден, берем первый созданный бизнес
        var firstBusiness = await _context.Businesses
            .IgnoreQueryFilters()
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        return firstBusiness?.Id;
    }
}

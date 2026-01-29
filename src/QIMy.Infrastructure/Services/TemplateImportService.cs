using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Сервис для импорта данных из бизнеса-шаблона в текущий бизнес
/// </summary>
public class TemplateImportService
{
    private readonly ApplicationDbContext _context;
    private const int TEMPLATE_BUSINESS_ID = 1; // ID бизнеса "Шаблон"

    public TemplateImportService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Generic Methods

    /// <summary>
    /// Получить доступные записи из шаблона (которых еще нет в текущем бизнесе)
    /// </summary>
    public async Task<List<T>> GetAvailableFromTemplateAsync<T>(
        int currentBusinessId,
        Func<T, string> keySelector,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, IMustHaveBusiness
    {
        // Получаем коды/ключи существующих записей в текущем бизнесе
        var existingKeys = await _context.Set<T>()
            .Where(x => x.BusinessId == currentBusinessId && !x.IsDeleted)
            .Select(x => keySelector(x))
            .ToListAsync(cancellationToken);

        // Получаем все записи из шаблона, которых еще нет
        var availableFromTemplate = await _context.Set<T>()
            .IgnoreQueryFilters()
            .Where(x => x.BusinessId == TEMPLATE_BUSINESS_ID && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        return availableFromTemplate
            .Where(x => !existingKeys.Contains(keySelector(x)))
            .ToList();
    }

    /// <summary>
    /// Добавить выбранные записи из шаблона в текущий бизнес
    /// </summary>
    public async Task<int> AddSelectedFromTemplateAsync<T>(
        int currentBusinessId,
        int[] selectedIds,
        Func<T, string> keySelector,
        CancellationToken cancellationToken = default)
        where T : BaseEntity, IMustHaveBusiness, new()
    {
        // Проверка на пустой массив
        if (selectedIds == null || selectedIds.Length == 0)
            return 0;

        // Получаем выбранные записи из шаблона
        var templateRecords = await _context.Set<T>()
            .IgnoreQueryFilters()
            .Where(x => x.BusinessId == TEMPLATE_BUSINESS_ID &&
                       selectedIds.Contains(x.Id) &&
                       !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!templateRecords.Any())
            return 0;

        // Получаем существующие ключи для проверки дубликатов
        var existingKeys = await _context.Set<T>()
            .Where(x => x.BusinessId == currentBusinessId && !x.IsDeleted)
            .Select(x => keySelector(x))
            .ToListAsync(cancellationToken);

        var added = 0;
        foreach (var template in templateRecords)
        {
            var key = keySelector(template);

            // Проверяем на дубликат
            if (existingKeys.Contains(key))
                continue;

            // Создаем копию для текущего бизнеса
            var newRecord = CloneEntity(template, currentBusinessId);

            await _context.Set<T>().AddAsync(newRecord, cancellationToken);
            added++;
        }

        if (added > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return added;
    }

    #endregion

    #region Specialized Methods for Each Entity

    // Currencies
    public Task<List<Currency>> GetAvailableCurrenciesAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<Currency>(businessId, c => c.Code, ct);

    public Task<int> AddCurrenciesFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<Currency>(businessId, ids, c => c.Code, ct);

    // TaxRates
    public Task<List<TaxRate>> GetAvailableTaxRatesAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<TaxRate>(businessId, r => $"{r.Rate}_{r.Name}", ct);

    public Task<int> AddTaxRatesFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<TaxRate>(businessId, ids, r => $"{r.Rate}_{r.Name}", ct);

    // ClientAreas
    public Task<List<ClientArea>> GetAvailableClientAreasAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<ClientArea>(businessId, a => a.Code, ct);

    public Task<int> AddClientAreasFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<ClientArea>(businessId, ids, a => a.Code, ct);

    // ClientTypes
    public Task<List<ClientType>> GetAvailableClientTypesAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<ClientType>(businessId, t => t.Code, ct);

    public Task<int> AddClientTypesFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<ClientType>(businessId, ids, t => t.Code, ct);

    // Accounts
    public Task<List<Account>> GetAvailableAccountsAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<Account>(businessId, a => a.AccountNumber, ct);

    public Task<int> AddAccountsFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<Account>(businessId, ids, a => a.AccountNumber, ct);

    // Units
    public Task<List<Unit>> GetAvailableUnitsAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<Unit>(businessId, u => u.ShortName, ct);

    public Task<int> AddUnitsFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<Unit>(businessId, ids, u => u.ShortName, ct);

    // PaymentMethods
    public Task<List<PaymentMethod>> GetAvailablePaymentMethodsAsync(int businessId, CancellationToken ct = default)
        => GetAvailableFromTemplateAsync<PaymentMethod>(businessId, p => p.Name, ct);

    public Task<int> AddPaymentMethodsFromTemplateAsync(int businessId, int[] ids, CancellationToken ct = default)
        => AddSelectedFromTemplateAsync<PaymentMethod>(businessId, ids, p => p.Name, ct);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Клонирует сущность для нового бизнеса
    /// </summary>
    private T CloneEntity<T>(T source, int newBusinessId) where T : BaseEntity, IMustHaveBusiness, new()
    {
        var clone = new T
        {
            BusinessId = newBusinessId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Копируем специфичные для типа свойства
        switch (source)
        {
            case Currency currency:
                var currencyClone = clone as Currency;
                currencyClone!.Code = currency.Code;
                currencyClone.Name = currency.Name;
                currencyClone.Symbol = currency.Symbol;
                currencyClone.ExchangeRate = currency.ExchangeRate;
                currencyClone.IsDefault = false; // У нового бизнеса будет своя дефолтная валюта
                break;

            case TaxRate taxRate:
                var taxRateClone = clone as TaxRate;
                taxRateClone!.Name = taxRate.Name;
                taxRateClone.Rate = taxRate.Rate;
                taxRateClone.IsDefault = false;
                break;

            case ClientArea area:
                var areaClone = clone as ClientArea;
                areaClone!.Code = area.Code;
                areaClone.Name = area.Name;
                areaClone.Description = area.Description;
                break;

            case ClientType type:
                var typeClone = clone as ClientType;
                typeClone!.Code = type.Code;
                typeClone.Name = type.Name;
                typeClone.Description = type.Description;
                break;

            case Account account:
                var accountClone = clone as Account;
                accountClone!.AccountNumber = account.AccountNumber;
                accountClone.Name = account.Name;
                accountClone.AccountCode = account.AccountCode;
                accountClone.IsForServices = account.IsForServices;
                accountClone.Comment = account.Comment;
                // Внимание: ClientAreaId и DefaultTaxRateId НЕ копируем (нужен mapping)
                break;

            case Unit unit:
                var unitClone = clone as Unit;
                unitClone!.Name = unit.Name;
                unitClone.ShortName = unit.ShortName;
                break;

            case PaymentMethod payment:
                var paymentClone = clone as PaymentMethod;
                paymentClone!.Name = payment.Name;
                break;
        }

        return clone;
    }

    #endregion
}

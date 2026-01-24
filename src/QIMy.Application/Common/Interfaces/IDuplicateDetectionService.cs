using QIMy.Application.Common.Models;
using QIMy.Core.Entities;

namespace QIMy.Application.Common.Interfaces;

/// <summary>
/// Сервис для обнаружения дубликатов в системе
/// Проверяет Client, Product, Supplier, Invoice и другие сущности
/// </summary>
public interface IDuplicateDetectionService
{
    /// <summary>
    /// Проверить дубликат клиента по имени
    /// </summary>
    Task<DuplicateDetectionResult?> CheckClientDuplicateAsync(
        string clientName,
        string? clientCode = null,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат товара по названию
    /// </summary>
    Task<DuplicateDetectionResult?> CheckProductDuplicateAsync(
        string productName,
        string? productCode = null,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат поставщика по имени
    /// </summary>
    Task<DuplicateDetectionResult?> CheckSupplierDuplicateAsync(
        string supplierName,
        string? vatNumber = null,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат счета по номеру
    /// </summary>
    Task<DuplicateDetectionResult?> CheckInvoiceDuplicateAsync(
        string invoiceNumber,
        int businessId,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат входящего счета по номеру
    /// </summary>
    Task<DuplicateDetectionResult?> CheckExpenseInvoiceDuplicateAsync(
        string invoiceNumber,
        int businessId,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат валюты по коду
    /// </summary>
    Task<DuplicateDetectionResult?> CheckCurrencyDuplicateAsync(
        string currencyCode,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат налоговой ставки
    /// </summary>
    Task<DuplicateDetectionResult?> CheckTaxRateDuplicateAsync(
        decimal rate,
        string? country = null,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Проверить дубликат по кастомному критерию
    /// </summary>
    Task<DuplicateDetectionResult?> CheckCustomDuplicateAsync<T>(
        Func<IQueryable<T>, Task<bool>> checkFunc,
        string entityTypeName,
        string fieldName,
        string fieldValue,
        CancellationToken ct = default) where T : BaseEntity;

    /// <summary>
    /// Проверить дубликат контрагента по KtoNr (PersonenIndexEntry)
    /// </summary>
    Task<DuplicateDetectionResult?> CheckPersonenIndexDuplicateAsync(
        string ktoNr,
        int? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Получить детали дубликата по ID
    /// </summary>
    Task<string> GetDuplicateDetailsAsync(string entityType, int entityId, CancellationToken ct = default);
}

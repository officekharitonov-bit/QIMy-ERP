namespace QIMy.Core.Entities;

/// <summary>
/// Аномалии обнаруженные AI
/// </summary>
public class AnomalyAlert : BaseEntity
{
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int? ExpenseInvoiceId { get; set; }
    public ExpenseInvoice? ExpenseInvoice { get; set; }

    /// <summary>
    /// Тип аномалии
    /// </summary>
    public AnomalyType Type { get; set; }

    /// <summary>
    /// Severity (0.0-1.0)
    /// </summary>
    public decimal Severity { get; set; }

    /// <summary>
    /// Описание проблемы
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Рекомендация AI
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Решено ли
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Как решено
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Когда решено
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}

public enum AnomalyType
{
    UnusualAmount = 1,      // Необычная сумма
    FrequencyAnomaly = 2,   // Необычная частота
    NewSupplier = 3,        // Новый поставщик
    DuplicateSuspected = 4, // Подозрение на дубликат
    PriceIncrease = 5,      // Значительное увеличение цены
    UnusualTiming = 6,      // Необычное время
    FraudSuspected = 7      // Подозрение на fraud
}

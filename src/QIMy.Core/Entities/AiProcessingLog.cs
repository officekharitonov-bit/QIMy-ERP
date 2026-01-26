namespace QIMy.Core.Entities;

/// <summary>
/// Логи AI обработки для мониторинга и улучшения моделей
/// </summary>
public class AiProcessingLog : BaseEntity
{
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    
    public int? ExpenseInvoiceId { get; set; }
    public ExpenseInvoice? ExpenseInvoice { get; set; }
    
    /// <summary>
    /// Тип AI сервиса: "OCR", "Classification", "Matching", "Approval", "Anomaly"
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Raw входные данные (JSON)
    /// </summary>
    public string? RawInput { get; set; }
    
    /// <summary>
    /// Ответ AI (JSON)
    /// </summary>
    public string AiResponse { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence score (0.0-1.0)
    /// </summary>
    public decimal ConfidenceScore { get; set; }
    
    /// <summary>
    /// Был ли принят пользователем
    /// </summary>
    public bool WasAcceptedByUser { get; set; }
    
    /// <summary>
    /// Коррекция пользователя (если отклонил)
    /// </summary>
    public string? UserCorrection { get; set; }
    
    /// <summary>
    /// Время обработки
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }
    
    /// <summary>
    /// Стоимость API вызова (EUR)
    /// </summary>
    public decimal Cost { get; set; }
}

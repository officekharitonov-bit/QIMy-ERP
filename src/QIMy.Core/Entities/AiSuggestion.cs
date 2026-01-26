namespace QIMy.Core.Entities;

/// <summary>
/// AI предложения для invoices
/// </summary>
public class AiSuggestion : BaseEntity
{
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    
    public int? ExpenseInvoiceId { get; set; }
    public ExpenseInvoice? ExpenseInvoice { get; set; }
    
    /// <summary>
    /// Тип предложения: "Steuercode", "Account", "Supplier", "Approval"
    /// </summary>
    public string SuggestionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Предложенное значение
    /// </summary>
    public string SuggestedValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Confidence (0.0-1.0)
    /// </summary>
    public decimal Confidence { get; set; }
    
    /// <summary>
    /// Объяснение reasoning
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;
    
    /// <summary>
    /// Был ли принят
    /// </summary>
    public bool WasAccepted { get; set; }
    
    /// <summary>
    /// Когда принят
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// Альтернативные значения (JSON array)
    /// </summary>
    public string? Alternatives { get; set; }
}

namespace QIMy.Core.Entities;

/// <summary>
/// AI configuration per business
/// </summary>
public class AiConfiguration : BaseEntity
{
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    
    /// <summary>
    /// Включить автоматический OCR для новых документов
    /// </summary>
    public bool EnableAutoOcr { get; set; } = true;
    
    /// <summary>
    /// Включить автоматическую классификацию (Steuercode, Account)
    /// </summary>
    public bool EnableAutoClassification { get; set; } = true;
    
    /// <summary>
    /// Включить auto-approval для низкорисковых invoices
    /// </summary>
    public bool EnableAutoApproval { get; set; } = false;
    
    /// <summary>
    /// Порог суммы для auto-approval (EUR)
    /// </summary>
    public decimal AutoApprovalThreshold { get; set; } = 100m;
    
    /// <summary>
    /// Минимальный confidence score для принятия AI suggestions (0.0-1.0)
    /// </summary>
    public decimal MinConfidenceScore { get; set; } = 0.7m;
    
    /// <summary>
    /// Предпочитаемый язык для explanations
    /// </summary>
    public string PreferredLanguage { get; set; } = "de";
    
    /// <summary>
    /// Включить anomaly detection
    /// </summary>
    public bool EnableAnomalyDetection { get; set; } = true;
    
    /// <summary>
    /// Включить AI chat assistant
    /// </summary>
    public bool EnableChatAssistant { get; set; } = true;
}

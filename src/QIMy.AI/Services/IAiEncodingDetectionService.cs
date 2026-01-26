namespace QIMy.AI.Services;

/// <summary>
/// AI-enhanced encoding detection service
/// </summary>
public interface IAiEncodingDetectionService
{
    /// <summary>
    /// Определяет кодировку файла с ML и confidence scoring
    /// </summary>
    Task<EncodingDetectionResult> DetectEncodingAsync(Stream stream, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Определяет кодировку из byte array
    /// </summary>
    Task<EncodingDetectionResult> DetectEncodingAsync(byte[] data, CancellationToken cancellationToken = default);
}

public class EncodingDetectionResult
{
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
    
    /// <summary>
    /// Confidence score (0.0-1.0)
    /// </summary>
    public decimal Confidence { get; set; }
    
    /// <summary>
    /// Метод определения: "BOM", "Statistical", "ML", "Fallback"
    /// </summary>
    public string DetectionMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Дополнительная информация
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// Альтернативные кодировки (если несколько подходят)
    /// </summary>
    public List<AlternativeEncoding> Alternatives { get; set; } = new();
}

public class AlternativeEncoding
{
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}

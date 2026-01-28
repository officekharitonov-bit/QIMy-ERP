namespace QIMy.AI.Services;

/// <summary>
/// AI-powered smart column mapping service
/// Maps CSV columns to entity properties using fuzzy matching and ML
/// </summary>
public interface IAiColumnMappingService
{
    /// <summary>
    /// Автоматически определяет соответствие колонок CSV к свойствам сущности
    /// </summary>
    Task<ColumnMappingResult> MapColumnsAsync<TEntity>(
        string[] csvHeaders,
        CancellationToken cancellationToken = default) where TEntity : class;
    
    /// <summary>
    /// Определяет соответствие с учетом образца данных (первые строки)
    /// </summary>
    Task<ColumnMappingResult> MapColumnsWithSampleDataAsync<TEntity>(
        string[] csvHeaders,
        List<string[]> sampleRows,
        CancellationToken cancellationToken = default) where TEntity : class;
}

public class ColumnMappingResult
{
    /// <summary>
    /// Словарь: CSV Column Index → Entity Property Name
    /// </summary>
    public Dictionary<int, string> Mappings { get; set; } = new();
    
    /// <summary>
    /// Confidence per mapping (0.0-1.0)
    /// </summary>
    public Dictionary<int, decimal> Confidences { get; set; } = new();
    
    /// <summary>
    /// Unmapped CSV columns (no match found)
    /// </summary>
    public List<UnmappedColumn> UnmappedColumns { get; set; } = new();
    
    /// <summary>
    /// Unmapped entity properties (required fields missing in CSV)
    /// </summary>
    public List<string> UnmappedProperties { get; set; } = new();
    
    /// <summary>
    /// Overall confidence score (0.0-1.0)
    /// </summary>
    public decimal OverallConfidence { get; set; }
    
    /// <summary>
    /// Warnings/suggestions for user
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

public class UnmappedColumn
{
    public int ColumnIndex { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string? SuggestedProperty { get; set; }
    public decimal SuggestionConfidence { get; set; }
}

namespace QIMy.AI.Services;

/// <summary>
/// AI-powered duplicate detection service using fuzzy matching
/// </summary>
public interface IAiDuplicateDetectionService
{
    /// <summary>
    /// Проверяет на дубликаты по нескольким критериям
    /// </summary>
    Task<DuplicateDetectionResult> DetectDuplicatesAsync<TEntity>(
        TEntity entity,
        IEnumerable<TEntity> existingEntities,
        DuplicateDetectionOptions? options = null,
        CancellationToken cancellationToken = default) where TEntity : class;
    
    /// <summary>
    /// Находит потенциальные дубликаты для клиента
    /// </summary>
    Task<List<DuplicateMatch>> FindDuplicateClientsAsync(
        string companyName,
        string? vatNumber = null,
        string? email = null,
        string? phone = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Находит потенциальные дубликаты для поставщика
    /// </summary>
    Task<List<DuplicateMatch>> FindDuplicateSuppliersAsync(
        string companyName,
        string? vatNumber = null,
        string? email = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Находит потенциальные дубликаты счетов
    /// </summary>
    Task<List<DuplicateMatch>> FindDuplicateInvoicesAsync(
        string invoiceNumber,
        int? clientId = null,
        decimal? amount = null,
        DateTime? date = null,
        CancellationToken cancellationToken = default);
}

public class DuplicateDetectionResult
{
    /// <summary>
    /// Найдены ли дубликаты
    /// </summary>
    public bool HasDuplicates { get; set; }
    
    /// <summary>
    /// Список найденных дубликатов
    /// </summary>
    public List<DuplicateMatch> Duplicates { get; set; } = new();
    
    /// <summary>
    /// Общий confidence score (0.0-1.0)
    /// </summary>
    public decimal OverallConfidence { get; set; }
    
    /// <summary>
    /// Рекомендация: Block, Warn, Allow
    /// </summary>
    public DuplicateAction RecommendedAction { get; set; }
    
    /// <summary>
    /// Объяснение для пользователя
    /// </summary>
    public string? Explanation { get; set; }
}

public class DuplicateMatch
{
    /// <summary>
    /// ID существующей сущности
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Название/описание сущности
    /// </summary>
    public string EntityDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Match score (0.0-1.0)
    /// </summary>
    public decimal MatchScore { get; set; }
    
    /// <summary>
    /// Какие поля совпали
    /// </summary>
    public List<FieldMatch> MatchedFields { get; set; } = new();
    
    /// <summary>
    /// Тип дубликата: Exact, Fuzzy, Suspected
    /// </summary>
    public DuplicateType Type { get; set; }
    
    /// <summary>
    /// Почему считается дубликатом
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

public class FieldMatch
{
    public string FieldName { get; set; } = string.Empty;
    public string? ExistingValue { get; set; }
    public string? NewValue { get; set; }
    public decimal SimilarityScore { get; set; } // 0.0-1.0
    public bool IsExactMatch { get; set; }
}

public class DuplicateDetectionOptions
{
    /// <summary>
    /// Минимальный threshold для exact duplicate (default: 0.95)
    /// </summary>
    public decimal ExactThreshold { get; set; } = 0.95m;
    
    /// <summary>
    /// Минимальный threshold для fuzzy duplicate (default: 0.75)
    /// </summary>
    public decimal FuzzyThreshold { get; set; } = 0.75m;
    
    /// <summary>
    /// Поля для проверки (если null, проверяются все)
    /// </summary>
    public List<string>? FieldsToCheck { get; set; }
    
    /// <summary>
    /// Веса полей для расчета общего score
    /// </summary>
    public Dictionary<string, decimal> FieldWeights { get; set; } = new();
    
    /// <summary>
    /// Игнорировать регистр
    /// </summary>
    public bool IgnoreCase { get; set; } = true;
    
    /// <summary>
    /// Игнорировать пробелы и специальные символы
    /// </summary>
    public bool IgnoreWhitespace { get; set; } = true;
}

public enum DuplicateType
{
    /// <summary>
    /// 100% совпадение
    /// </summary>
    Exact,
    
    /// <summary>
    /// Очень похоже (>90%)
    /// </summary>
    Fuzzy,
    
    /// <summary>
    /// Подозрительно похоже (75-90%)
    /// </summary>
    Suspected,
    
    /// <summary>
    /// Может быть дубликат (60-75%)
    /// </summary>
    Possible
}

public enum DuplicateAction
{
    /// <summary>
    /// Заблокировать создание
    /// </summary>
    Block,
    
    /// <summary>
    /// Предупредить пользователя
    /// </summary>
    Warn,
    
    /// <summary>
    /// Разрешить создание
    /// </summary>
    Allow
}

namespace QIMy.Application.Common.Models;

/// <summary>
/// Результат проверки на дубликаты
/// </summary>
public class DuplicateDetectionResult
{
    /// <summary>
    /// Есть ли найденные дубликаты
    /// </summary>
    public bool HasDuplicates { get; set; }

    /// <summary>
    /// Тип дубликата (Client, Product, Supplier и т.д.)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Поле, по которому найден дубликат
    /// </summary>
    public string? DuplicateField { get; set; }

    /// <summary>
    /// Значение, которое уже существует
    /// </summary>
    public string? DuplicateValue { get; set; }

    /// <summary>
    /// ID существующей записи
    /// </summary>
    public int ExistingEntityId { get; set; }

    /// <summary>
    /// Детали существующей записи (для отображения пользователю)
    /// </summary>
    public string? ExistingEntityDetails { get; set; }

    /// <summary>
    /// Уровень серьёзности (Info, Warning, Error)
    /// </summary>
    public DuplicateSeverity Severity { get; set; } = DuplicateSeverity.Warning;

    /// <summary>
    /// Сообщение для пользователя
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Количество найденных дубликатов
    /// </summary>
    public int DuplicateCount { get; set; }

    /// <summary>
    /// Был ли это проигнорировано раньше?
    /// </summary>
    public bool WasIgnoredBefore { get; set; }

    /// <summary>
    /// Требуется ли двойное подтверждение?
    /// </summary>
    public bool RequireDoubleConfirmation { get; set; } = true;
}

/// <summary>
/// Уровень серьёзности обнаруженного дубликата
/// </summary>
public enum DuplicateSeverity
{
    /// <summary>Информационное сообщение</summary>
    Info = 0,

    /// <summary>Предупреждение (дубликат существует)</summary>
    Warning = 1,

    /// <summary>Ошибка (запретить создание)</summary>
    Error = 2
}

/// <summary>
/// Результат с информацией о дубликатах при создании/обновлении
/// </summary>
public class DuplicateWarningResponse<T> where T : class
{
    /// <summary>
    /// Результат создания/обновления (если был игнорирован дубликат)
    /// </summary>
    public T? Entity { get; set; }

    /// <summary>
    /// ID созданной/обновленной записи
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Обнаруженный дубликат
    /// </summary>
    public DuplicateDetectionResult? Duplicate { get; set; }

    /// <summary>
    /// Был ли создан несмотря на дубликат?
    /// </summary>
    public bool CreatedWithDuplicate { get; set; }

    /// <summary>
    /// Требуется ли подтверждение от пользователя
    /// </summary>
    public bool RequiresUserConfirmation { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Статус операции
    /// </summary>
    public DuplicateOperationStatus Status { get; set; }
}

/// <summary>
/// Статус операции с проверкой дубликатов
/// </summary>
public enum DuplicateOperationStatus
{
    /// <summary>Успешно создано без дубликатов</summary>
    SuccessNoDuplicate = 0,

    /// <summary>Требуется подтверждение пользователя</summary>
    DuplicateWarning = 1,

    /// <summary>Успешно создано несмотря на дубликат (после подтверждения)</summary>
    SuccessWithDuplicate = 2,

    /// <summary>Отклонено из-за дубликата</summary>
    RejectedDuplicate = 3,

    /// <summary>Ошибка при проверке</summary>
    Error = 4
}

/// <summary>
/// Request для игнорирования дубликата и создания записи
/// </summary>
public class IgnoreDuplicateRequest
{
    /// <summary>
    /// Был ли первый раз игнорирован дубликат
    /// </summary>
    public bool FirstIgnore { get; set; }

    /// <summary>
    /// Это второе подтверждение (окончательное)?
    /// </summary>
    public bool DoubleConfirmed { get; set; }

    /// <summary>
    /// ID дубликата, который игнорируется
    /// </summary>
    public int DuplicateEntityId { get; set; }
}

namespace QIMy.Core.Entities;

/// <summary>
/// Cash Entry (Кассовая операция)
/// Записи о приходе/расходе наличных в кассе
/// </summary>
public class CashEntry : BaseEntity
{
    /// <summary>
    /// Бизнес, к которому относится запись
    /// </summary>
    public int BusinessId { get; set; }
    public virtual Business Business { get; set; } = null!;

    /// <summary>
    /// Касса, в которой произошла операция
    /// </summary>
    public int CashBoxId { get; set; }
    public virtual CashBox CashBox { get; set; } = null!;

    /// <summary>
    /// Номер записи (уникален в рамках CashBox)
    /// Формат: КА-2026-0001, КА-2026-0002 и т.д.
    /// </summary>
    public required string EntryNumber { get; set; }

    /// <summary>
    /// Дата операции
    /// </summary>
    public required DateTime EntryDate { get; set; }

    /// <summary>
    /// Тип операции
    /// </summary>
    public required CashEntryType EntryType { get; set; }

    /// <summary>
    /// Сумма операции
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Описание операции
    /// Пример: "Оплата счета AR-2026-0001", "Зарплата сотрудников"
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Валюта (ISO 4217 код)
    /// </summary>
    public required string CurrencyCode { get; set; } = "EUR";

    /// <summary>
    /// Контрагент (если применимо)
    /// </summary>
    public int? PersonenIndexEntryId { get; set; }
    public virtual PersonenIndexEntry? PersonenIndexEntry { get; set; }

    /// <summary>
    /// Связанный документ (Invoice, ExpenseInvoice и т.д.)
    /// </summary>
    public int? LinkedDocumentId { get; set; }

    /// <summary>
    /// Тип связанного документа
    /// </summary>
    public string? LinkedDocumentType { get; set; }

    /// <summary>
    /// Номер связанного документа (для быстрого поиска)
    /// </summary>
    public string? LinkedDocumentNumber { get; set; }

    /// <summary>
    /// Счет (куда поступили/откуда взяли деньги)
    /// Пример: "Bank", "AR Payment", "Supplier Payment"
    /// </summary>
    public string? CounterAccount { get; set; }

    /// <summary>
    /// Статус записи
    /// </summary>
    public CashEntryStatus Status { get; set; } = CashEntryStatus.Pending;

    /// <summary>
    /// Дата утверждения записи
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// ID пользователя, утвердившего запись
    /// </summary>
    public string? ApprovedByUserId { get; set; }

    /// <summary>
    /// Примечание
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Дата создания записи в системе
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ID пользователя, создавшего запись
    /// </summary>
    public string? CreatedByUserId { get; set; }

    /// <summary>
    /// Флаг для квартальной архивизации
    /// </summary>
    public int Quarter { get; set; }
    public int Year { get; set; }
}

/// <summary>
/// Касса (Cash Box)
/// Представляет физическую кассу или кассовый счет
/// </summary>
public class CashBox : BaseEntity
{
    /// <summary>
    /// Бизнес, к которому относится касса
    /// </summary>
    public int BusinessId { get; set; }
    public virtual Business Business { get; set; } = null!;

    /// <summary>
    /// Номер/код кассы
    /// Пример: "KA-001", "REGISTER-01"
    /// </summary>
    public required string CashBoxNumber { get; set; }

    /// <summary>
    /// Название кассы
    /// Пример: "Main Cash Register", "Office Safe"
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Описание (где находится и т.д.)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Тип кассы
    /// </summary>
    public CashBoxType CashBoxType { get; set; }

    /// <summary>
    /// Основная валюта кассы
    /// </summary>
    public required string CurrencyCode { get; set; } = "EUR";

    /// <summary>
    /// Текущий остаток в кассе
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Статус кассы
    /// </summary>
    public CashBoxStatus Status { get; set; } = CashBoxStatus.Active;

    /// <summary>
    /// Дата открытия кассы
    /// </summary>
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата закрытия кассы (если закрыта)
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Ответственное лицо (сотрудник)
    /// </summary>
    public string? ResponsiblePerson { get; set; }

    /// <summary>
    /// Записи в этой кассе
    /// </summary>
    public virtual ICollection<CashEntry> Entries { get; set; } = new List<CashEntry>();

    /// <summary>
    /// Дневные балансы
    /// </summary>
    public virtual ICollection<CashBookDay> DailyBooks { get; set; } = new List<CashBookDay>();
}

/// <summary>
/// Дневная кассовая книга (Daily Cash Book)
/// Итоговая запись за день на кассе
/// </summary>
public class CashBookDay : BaseEntity
{
    /// <summary>
    /// Касса
    /// </summary>
    public int CashBoxId { get; set; }
    public virtual CashBox CashBox { get; set; } = null!;

    /// <summary>
    /// Дата дня
    /// </summary>
    public required DateTime DayDate { get; set; }

    /// <summary>
    /// Начальный остаток (остаток на начало дня)
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Сумма приходов за день
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Сумма расходов за день
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Конечный остаток (отчет)
    /// Opening + Income - Expense
    /// </summary>
    public decimal ExpectedClosingBalance { get; set; }

    /// <summary>
    /// Фактический остаток (инвентаризация)
    /// </summary>
    public decimal? ActualClosingBalance { get; set; }

    /// <summary>
    /// Разница между ожидаемым и фактическим остатком
    /// Положительное значение = излишек
    /// Отрицательное значение = недостаток
    /// </summary>
    public decimal? Variance { get; set; }

    /// <summary>
    /// Статус дневника
    /// </summary>
    public CashBookDayStatus Status { get; set; } = CashBookDayStatus.Pending;

    /// <summary>
    /// Дата утверждения дневника
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// ID пользователя, утвердившего дневник
    /// </summary>
    public string? ApprovedByUserId { get; set; }

    /// <summary>
    /// Примечание (объяснение разницы и т.д.)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Дата создания записи в системе
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Квартал и год для архивизации
    /// </summary>
    public int Quarter { get; set; }
    public int Year { get; set; }
}

/// <summary>
/// Тип кассовой операции
/// </summary>
public enum CashEntryType
{
    /// <summary>Поступление денег в кассу</summary>
    Income = 1,

    /// <summary>Выплата денег из кассы</summary>
    Expense = 2,

    /// <summary>Перевод денег на другой счет</summary>
    Transfer = 3,

    /// <summary>Корректировка (инвентаризация)</summary>
    Adjustment = 4,

    /// <summary>Возврат денег</summary>
    Refund = 5
}

/// <summary>
/// Статус кассовой записи
/// </summary>
public enum CashEntryStatus
{
    /// <summary>На одобрение</summary>
    Pending = 0,

    /// <summary>Одобрена</summary>
    Approved = 1,

    /// <summary>Отклонена</summary>
    Rejected = 2,

    /// <summary>В архиве</summary>
    Archived = 3
}

/// <summary>
/// Тип кассы
/// </summary>
public enum CashBoxType
{
    /// <summary>Физическая касса (наличные деньги)</summary>
    Physical = 1,

    /// <summary>Кассовый расчетный счет</summary>
    CashAccount = 2,

    /// <summary>Сейф</summary>
    Safe = 3,

    /// <summary>Кассовый аппарат</summary>
    Register = 4
}

/// <summary>
/// Статус кассы
/// </summary>
public enum CashBoxStatus
{
    /// <summary>Активна</summary>
    Active = 1,

    /// <summary>Неактивна</summary>
    Inactive = 2,

    /// <summary>Закрыта</summary>
    Closed = 3
}

/// <summary>
/// Статус дневной кассовой книги
/// </summary>
public enum CashBookDayStatus
{
    /// <summary>На утверждение</summary>
    Pending = 0,

    /// <summary>Утверждена</summary>
    Approved = 1,

    /// <summary>Требуется корректировка</summary>
    RequiresCorrection = 2,

    /// <summary>Скорректирована</summary>
    Corrected = 3,

    /// <summary>В архиве</summary>
    Archived = 4
}

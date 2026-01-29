using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Journal Entry (BUCHUNGSSCHRITTE) - Бухгалтерская проводка
/// Представляет двойную запись бухучета (Debit-Credit)
/// Каждая операция (Invoice, ExpenseInvoice, Payment, CashEntry) генерирует журналы
/// </summary>
public class JournalEntry : BaseEntity, IMustHaveBusiness
{
    /// <summary>
    /// Бизнес, к которому относится проводка
    /// </summary>
    public int BusinessId { get; set; }
    public virtual Business Business { get; set; } = null!;

    /// <summary>
    /// Номер проводки (уникален в рамках BusinessId)
    /// Формат: JE-2026-0001, JE-2026-0002 и т.д.
    /// </summary>
    public required string EntryNumber { get; set; }

    /// <summary>
    /// Дата проводки (дата документа, который создал проводку)
    /// </summary>
    public required DateTime EntryDate { get; set; }

    /// <summary>
    /// Описание проводки
    /// Пример: "Invoice AR-2026-0001" или "Expense ER-2026-0001"
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Тип источника проводки
    /// </summary>
    public required JournalEntrySourceType SourceType { get; set; }

    /// <summary>
    /// ID источника (InvoiceId, ExpenseInvoiceId, CashEntryId и т.д.)
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// Справочный номер источника (для быстрого поиска)
    /// Пример: "AR-2026-0001", "ER-2026-0001"
    /// </summary>
    public string? SourceReference { get; set; }

    /// <summary>
    /// Статус проводки
    /// </summary>
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Posted;

    /// <summary>
    /// Сумма дебета (всегда = сумме кредита для валидной проводки)
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Сумма кредита
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Валюта (ISO 4217 код: EUR, USD и т.д.)
    /// </summary>
    public required string CurrencyCode { get; set; } = "EUR";

    /// <summary>
    /// Дата создания проводки в системе
    /// </summary>
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public new DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ID пользователя, создавшего проводку
    /// </summary>
    public string? CreatedByUserId { get; set; }

    /// <summary>
    /// Флаг для квартальной архивизации
    /// </summary>
    public int Quarter { get; set; }

    public int Year { get; set; }

    /// <summary>
    /// Строки проводки (дебеты и кредиты)
    /// </summary>
    public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();

    /// <summary>
    /// Контрагент (если применимо)
    /// </summary>
    public int? PersonenIndexEntryId { get; set; }
    public virtual PersonenIndexEntry? PersonenIndexEntry { get; set; }
}

/// <summary>
/// Строка журнальной проводки (дебет или кредит)
/// </summary>
public class JournalEntryLine : BaseEntity
{
    /// <summary>
    /// ID журнальной проводки
    /// </summary>
    public int JournalEntryId { get; set; }
    public virtual JournalEntry JournalEntry { get; set; } = null!;

    /// <summary>
    /// Номер счета (СОР счет)
    /// Пример: 1000 (Bank), 4000 (Revenue), 5000 (Expense)
    /// </summary>
    public required string AccountCode { get; set; }

    /// <summary>
    /// Название счета (для аудита)
    /// Пример: "Bank Account", "Sales Revenue"
    /// </summary>
    public required string AccountName { get; set; }

    /// <summary>
    /// Является ли это дебет строкой
    /// true = Debit, false = Credit
    /// </summary>
    public bool IsDebit { get; set; }

    /// <summary>
    /// Сумма
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Описание строки (опционально)
    /// Пример: "Invoice AR-2026-0001 payment"
    /// </summary>
    public string? LineDescription { get; set; }

    /// <summary>
    /// Контрагент (если применимо)
    /// </summary>
    public int? PersonenIndexEntryId { get; set; }
    public virtual PersonenIndexEntry? PersonenIndexEntry { get; set; }

    /// <summary>
    /// Порядок строк в проводке (для сортировки)
    /// </summary>
    public int LineSequence { get; set; }
}

/// <summary>
/// Тип источника проводки
/// </summary>
public enum JournalEntrySourceType
{
    /// <summary>Исходящий счет (AR - Ausgangsrechnung)</summary>
    Invoice = 1,

    /// <summary>Входящий счет (ER - Eingangsrechnung)</summary>
    ExpenseInvoice = 2,

    /// <summary>Платеж по счету</summary>
    Payment = 3,

    /// <summary>Банковская выписка</summary>
    BankStatement = 4,

    /// <summary>Кассовая операция</summary>
    CashEntry = 5,

    /// <summary>Корректировка (ручная проводка)</summary>
    ManualAdjustment = 6,

    /// <summary>Возвратная накладная</summary>
    Return = 7,

    /// <summary>Ручная запись</summary>
    Manual = 8
}

/// <summary>
/// Статус журнальной проводки
/// </summary>
public enum JournalEntryStatus
{
    /// <summary>Черновик (не постирована)</summary>
    Draft = 0,

    /// <summary>Постирована (принята в учет)</summary>
    Posted = 1,

    /// <summary>Реверсирована</summary>
    Reversed = 2,

    /// <summary>Отменена</summary>
    Cancelled = 3,

    /// <summary>В архиве</summary>
    Archived = 4
}

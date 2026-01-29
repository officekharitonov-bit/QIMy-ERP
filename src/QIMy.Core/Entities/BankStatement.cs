using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Bank Statement (Банковская выписка)
/// Импортируется из CSV файлов различных банков
/// </summary>
public class BankStatement : BaseEntity, IMustHaveBusiness
{
    /// <summary>
    /// Бизнес, к которому относится выписка
    /// </summary>
    public int BusinessId { get; set; }
    public virtual Business Business { get; set; } = null!;

    /// <summary>
    /// Банковский счет
    /// </summary>
    public int BankAccountId { get; set; }
    public virtual BankAccount BankAccount { get; set; } = null!;

    /// <summary>
    /// Номер выписки (от банка)
    /// Пример: "2026-01-024"
    /// </summary>
    public required string StatementNumber { get; set; }

    /// <summary>
    /// Дата начала периода выписки
    /// </summary>
    public required DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания периода выписки
    /// </summary>
    public required DateTime EndDate { get; set; }

    /// <summary>
    /// Начальный остаток на начало периода
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Конечный остаток на конец периода (по выписке)
    /// </summary>
    public decimal ClosingBalance { get; set; }

    /// <summary>
    /// Сумма всех дебетов (приходов)
    /// </summary>
    public decimal TotalDebits { get; set; }

    /// <summary>
    /// Сумма всех кредитов (расходов)
    /// </summary>
    public decimal TotalCredits { get; set; }

    /// <summary>
    /// Валюта выписки
    /// </summary>
    public required string CurrencyCode { get; set; } = "EUR";

    /// <summary>
    /// Статус выписки
    /// </summary>
    public BankStatementStatus Status { get; set; } = BankStatementStatus.New;

    /// <summary>
    /// Дата импорта в систему
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последней сверки с учетом
    /// </summary>
    public DateTime? ReconciliedAt { get; set; }

    /// <summary>
    /// Флаг для квартальной архивизации
    /// </summary>
    public int Quarter { get; set; }
    public int Year { get; set; }

    /// <summary>
    /// Строки выписки (транзакции)
    /// </summary>
    public virtual ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();

    /// <summary>
    /// Сверки (связи с документами)
    /// </summary>
    public virtual ICollection<BankReconciliation> Reconciliations { get; set; } = new List<BankReconciliation>();
}

/// <summary>
/// Строка банковской выписки (одна транзакция)
/// </summary>
public class BankStatementLine : BaseEntity
{
    /// <summary>
    /// ID выписки
    /// </summary>
    public int BankStatementId { get; set; }
    public virtual BankStatement BankStatement { get; set; } = null!;

    /// <summary>
    /// Дата транзакции
    /// </summary>
    public required DateTime TransactionDate { get; set; }

    /// <summary>
    /// Дата валютирования (если отличается от даты транзакции)
    /// </summary>
    public DateTime? ValueDate { get; set; }

    /// <summary>
    /// Сумма (положительная для прихода, отрицательная для расхода)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Код транзакции (SWIFT и т.д.)
    /// </summary>
    public string? TransactionCode { get; set; }

    /// <summary>
    /// Описание транзакции (от банка)
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Дополнительная информация о транзакции
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Противоположный счет (IBAN другой стороны)
    /// </summary>
    public string? CounterpartyIban { get; set; }

    /// <summary>
    /// Название противоположной стороны
    /// </summary>
    public string? CounterpartyName { get; set; }

    /// <summary>
    /// Порядок транзакции в выписке
    /// </summary>
    public int LineSequence { get; set; }

    /// <summary>
    /// Связанный документ (Invoice, ExpenseInvoice и т.д.)
    /// </summary>
    public int? LinkedDocumentId { get; set; }

    /// <summary>
    /// Тип связанного документа
    /// </summary>
    public string? LinkedDocumentType { get; set; }

    /// <summary>
    /// Статус сверки
    /// </summary>
    public BankLineReconciliationStatus ReconciliationStatus { get; set; } = BankLineReconciliationStatus.Unreconciled;

    /// <summary>
    /// Примечание (для ручной сверки)
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Банковская сверка (связь между выпиской и документом)
/// </summary>
public class BankReconciliation : BaseEntity
{
    /// <summary>
    /// ID выписки
    /// </summary>
    public int BankStatementId { get; set; }
    public virtual BankStatement BankStatement { get; set; } = null!;

    /// <summary>
    /// ID строки выписки
    /// </summary>
    public int? BankStatementLineId { get; set; }
    public virtual BankStatementLine? BankStatementLine { get; set; }

    /// <summary>
    /// Тип документа (Invoice, ExpenseInvoice, Payment и т.д.)
    /// </summary>
    public required string DocumentType { get; set; }

    /// <summary>
    /// ID документа
    /// </summary>
    public int DocumentId { get; set; }

    /// <summary>
    /// Номер документа (для быстрого поиска)
    /// </summary>
    public required string DocumentNumber { get; set; }

    /// <summary>
    /// Дата документа
    /// </summary>
    public required DateTime DocumentDate { get; set; }

    /// <summary>
    /// Сумма, сверенная
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Статус сверки
    /// </summary>
    public BankReconciliationStatus Status { get; set; } = BankReconciliationStatus.Pending;

    /// <summary>
    /// Дата создания сверки
    /// </summary>
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата утверждения сверки
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Примечание
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Статус банковской выписки
/// </summary>
public enum BankStatementStatus
{
    /// <summary>Новая (импортирована, не обработана)</summary>
    New = 0,

    /// <summary>В процессе сверки</summary>
    Reconciling = 1,

    /// <summary>Сверена (все строки обработаны)</summary>
    Reconciled = 2,

    /// <summary>Опубликована (готова к использованию)</summary>
    Posted = 3,

    /// <summary>В архиве</summary>
    Archived = 4
}

/// <summary>
/// Статус сверки строки выписки
/// </summary>
public enum BankLineReconciliationStatus
{
    /// <summary>Не сверена</summary>
    Unreconciled = 0,

    /// <summary>Частично сверена</summary>
    PartiallyReconciled = 1,

    /// <summary>Полностью сверена</summary>
    FullyReconciled = 2,

    /// <summary>Отклонена</summary>
    Rejected = 3
}

/// <summary>
/// Статус банковской сверки
/// </summary>
public enum BankReconciliationStatus
{
    /// <summary>На рассмотрении</summary>
    Pending = 0,

    /// <summary>Сверена</summary>
    Approved = 1,

    /// <summary>Отклонена</summary>
    Rejected = 2,

    /// <summary>В архиве</summary>
    Archived = 3
}

using QIMy.Core.Interfaces;

namespace QIMy.Core.Entities;

/// <summary>
/// Personen Index Entry - центральный справочник контрагентов
/// Это "телефонная книга" системы - единственный источник правды для данных контрагентов
///
/// Архитектура:
/// - Один контрагент может быть одновременно клиентом (AR) и поставщиком (ER)
/// - Kto-Nr (номер счета) определяет тип и назначение:
///   - 2xxxxx: Клиенты (AR - Ausgangsrechnungen)
///   - 3xxxxx: Поставщики (ER - Eingangsrechnungen)
///   - 4xxxxx: Смешанные (оба)
/// - TAG - короткое имя для быстрого ввода в ER/AR
/// - Страна определяет налоговый режим (EU-RATE)
/// - Предлагаемые счета автоматически подставляются в операции
/// </summary>
public class PersonenIndexEntry : BaseEntity, IMustHaveBusiness
{
    /// <summary>
    /// Бизнес (мультитенантность)
    /// </summary>
    public int BusinessId { get; set; }

    /// <summary>
    /// Номер счета (Kto-Nr) - уникальный ID контрагента
    /// 2xxxxx: Клиент (AR)
    /// 3xxxxx: Поставщик (ER)
    /// 4xxxxx: Оба (AR + ER)
    /// </summary>
    public string KtoNr { get; set; } = string.Empty;

    /// <summary>
    /// TAG - краткая аббревиатура для быстрого ввода (первые 5 букв названия)
    /// Используется вместо полного Kto-Nr при вводе операций
    /// </summary>
    public string TAG { get; set; } = string.Empty;

    /// <summary>
    /// Полное юридическое название (Nachname)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Контактное лицо (Vorname)
    /// </summary>
    public string? ContactPerson { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Адрес
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Город
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Почтовый индекс
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Код страны (AT, DE, BE и т.д. - Freifeld 01)
    /// Определяет налоговый режим через EU-RATE
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// UID-номер (НДС ID, VAT Number)
    /// Критически важен для отчетов в налоговую
    /// Формат: ATU12345678 и т.д.
    /// </summary>
    public string? UIDNumber { get; set; }

    /// <summary>
    /// Предлагаемый счет расхода (Lief-Vorschlag)
    /// Подставляется автоматически при вводе ER
    /// </summary>
    public int? SuggestedExpenseAccountId { get; set; }

    /// <summary>
    /// Предлагаемый счет дохода (Kunden-Vorschlag)
    /// Подставляется автоматически при вводе AR
    /// </summary>
    public int? SuggestedIncomeAccountId { get; set; }

    /// <summary>
    /// Номер филиала (для сетей с несколькими точками)
    /// </summary>
    public int? BranchNumber { get; set; }

    /// <summary>
    /// Цифровой код страны для внутренней сортировки
    /// </summary>
    public int? CountryNumber { get; set; }

    /// <summary>
    /// Тип контрагента:
    /// - Customer (только клиент AR)
    /// - Supplier (только поставщик ER)
    /// - Both (оба - AR и ER)
    /// </summary>
    public ContractorType ContractorType { get; set; } = ContractorType.Supplier;

    /// <summary>
    /// Является ли внутренним подразделением (филиал, отделение своей компании)
    /// </summary>
    public bool IsInternalDivision { get; set; } = false;

    /// <summary>
    /// Статус в системе
    /// </summary>
    public ContractorStatus Status { get; set; } = ContractorStatus.Active;

    // Navigation properties
    public Business? Business { get; set; }

    /// <summary>
    /// Связь с записью Клиента (если это AR контрагент)
    /// </summary>
    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    /// <summary>
    /// Связь с записью Поставщика (если это ER контрагент)
    /// </summary>
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// Предлагаемый счет расхода
    /// </summary>
    public Account? SuggestedExpenseAccount { get; set; }

    /// <summary>
    /// Предлагаемый счет дохода
    /// </summary>
    public Account? SuggestedIncomeAccount { get; set; }

    /// <summary>
    /// Страна контрагента (для применения налогов через EU-RATE)
    /// </summary>
    public Country? Country { get; set; }

    /// <summary>
    /// История входящих счетов (ER) от этого поставщика
    /// </summary>
    public ICollection<ExpenseInvoice> ExpenseInvoices { get; set; } = new List<ExpenseInvoice>();

    /// <summary>
    /// История исходящих счетов (AR) этому клиенту
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

/// <summary>
/// Тип контрагента в Personen Index
/// </summary>
public enum ContractorType
{
    /// <summary>Только клиент (AR)</summary>
    Customer = 1,

    /// <summary>Только поставщик (ER)</summary>
    Supplier = 2,

    /// <summary>Оба - и клиент, и поставщик (AR + ER)</summary>
    Both = 3
}

/// <summary>
/// Статус контрагента в системе
/// </summary>
public enum ContractorStatus
{
    /// <summary>Активный контрагент</summary>
    Active = 1,

    /// <summary>Неактивный (архивированный)</summary>
    Inactive = 2,

    /// <summary>На проверке</summary>
    Pending = 3,

    /// <summary>Заблокирован</summary>
    Blocked = 4
}

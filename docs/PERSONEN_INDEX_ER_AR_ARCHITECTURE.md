# QIMy Архитектура: Персонен Индекс (Personen Index) - ER/AR Интеграция

## 📋 Обзор системы

Система построена по классическому принципу **"Звезда"** (Star Schema):

```
                    ┌─────────────────────┐
                    │  Personen Index     │
                    │  (Справочник "мозг")│
                    └──────────┬──────────┘
                              /│\
                    ┌────────/ │ \────────┐
                    │         │         │
                    ▼         ▼         ▼
                   AR        ER       EU-RATE
            (Исход.счета) (Входящ.) (Налоги)
            Ausgangs-     Eingangs-
            rechnungen    rechnungen
```

**В центре** → **Personen Index** - единственный источник правды (Single Source of Truth)
**Вокруг** → **ER (Eingangsrechnungen)**, **AR (Ausgangsrechnungen)**, **EU-RATE** (налоги)

---

## 🏛️ Архитектурные слои

### 1. Personen Index Entry (Справочник контрагентов)

**Entity**: `PersonenIndexEntry`

```csharp
public class PersonenIndexEntry : BaseEntity
{
    // Идентификация
    public string KtoNr { get; set; }           // Номер счета (2xxxxx, 3xxxxx, 4xxxxx)
    public string TAG { get; set; }             // Краткая аббревиатура (для быстрого ввода)

    // Базовые данные
    public string CompanyName { get; set; }     // Полное юридическое название
    public string? ContactPerson { get; set; }  // ФИО контакта
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    // Налоговые данные
    public string CountryCode { get; set; }     // AT, DE, BE... (определяет налоги)
    public string? UIDNumber { get; set; }      // UID/VAT ID (ATU12345678...)

    // Рекомендуемые счета
    public int? SuggestedExpenseAccountId { get; set; }  // Для ER (расходы)
    public int? SuggestedIncomeAccountId { get; set; }   // Для AR (доходы)

    // Классификация
    public ContractorType ContractorType { get; set; }   // Customer, Supplier, Both
    public ContractorStatus Status { get; set; }          // Active, Inactive, Pending...
}
```

**Типы контрагентов:**
- `Customer (200000-299999)` - только клиент, AR документы
- `Supplier (300000-399999)` - только поставщик, ER документы
- `Both (400000-499999)` - оба роли одновременно

---

### 2. ER (Входящие счета) - Eingangsrechnungen

**Entity**: `ExpenseInvoice`

```csharp
public class ExpenseInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }

    // Связь с контрагентом
    public int SupplierId { get; set; }                    // Прямая ссылка на Supplier
    public int? PersonenIndexEntryId { get; set; }        // ✨ Ссылка на Personen Index

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public PersonenIndexEntry? PersonenIndexEntry { get; set; }
    public ICollection<ExpenseInvoiceItem> Items { get; set; }
}
```

**Flow (как работает):**
```
1. Приходит счет → Пользователь вводит TAG поставщика (например, "MonoOst")
2. Система ищет в Personen Index по TAG
3. Подтягивает данные:
   - UID-номер (для налоговых отчетов)
   - CountryCode (AT, DE, BE...)
   - SuggestedExpenseAccount (рекомендуемый счет 5030, 5050...)
   - Адрес, контакты
4. Система смотрит в EU-RATE по CountryCode
5. Берет актуальную налоговую ставку
6. Пересчитывает сумму с налогом
```

---

### 3. AR (Исходящие счета) - Ausgangsrechnungen

**Entity**: `Invoice`

```csharp
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }

    // Связь с контрагентом
    public int ClientId { get; set; }                      // Прямая ссылка на Client
    public int? PersonenIndexEntryId { get; set; }        // ✨ Ссылка на Personen Index

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation properties
    public PersonenIndexEntry? PersonenIndexEntry { get; set; }
    public ICollection<InvoiceItem> Items { get; set; }
}
```

**Flow (как работает):**
```
1. Создаем счет клиенту → Вводим TAG клиента (например, "AcmeCorp")
2. Система ищет в Personen Index по TAG
3. Подтягивает данные:
   - UID-номер клиента (для счета)
   - CountryCode (определяет применимый налог)
   - SuggestedIncomeAccount (рекомендуемый счет доходов)
4. На основе CountryCode система определяет налоговый режим:
   - AT (Австрия) → Стандартный НДС 20%
   - DE (Германия) → НДС 19%
   - Третьи страны → Может быть 0% (реверс НДС)
5. Подтягивает ставку из EU-RATE.csv
6. Пересчитывает сумму с правильным налогом
```

---

## 🔗 Взаимодействие таблиц (Data Flow)

### Шаг 1: Регистрация контрагента (Personen Index)

```
Новый поставщик "Monolith Ost GmbH"
        ↓
Заносим в Personen Index:
  - Kto-Nr: 300151 (поставщик)
  - TAG: MonoOst (для быстрого ввода)
  - CompanyName: Monolith Ost GmbH
  - CountryCode: DE (это Германия!)
  - UIDNumber: DE123456789
  - SuggestedExpenseAccount: 5030 (закупка товаров)
  - Status: Active
```

### Шаг 2: Входящий счет (ER)

```
Счет #2024/1234 от Monolith Ost приходит
        ↓
Вводим в ExpenseInvoice:
  - InvoiceNumber: "2024/1234"
  - InvoiceDate: 2024-01-20
  - SupplierId: 5 (ID записи в Supplier)
  - PersonenIndexEntryId: 42 (ID в Personen Index)
        ↓
Система читает PersonenIndexEntry #42:
  - UIDNumber: DE123456789 ✓ (для документов)
  - CountryCode: DE ✓ (для поиска налога)
  - SuggestedExpenseAccount: 5030 ✓ (автоматически применяется)
        ↓
Система идет в EU-RATE.csv:
  - Ищет: CountryCode = "DE"
  - Находит: VAT Rate = 19%
        ↓
Расчет налога:
  - SubTotal: 1000€
  - Tax (19%): 190€
  - Total: 1190€
```

### Шаг 3: Исходящий счет (AR)

```
Счет клиенту "Acme Corp Gmbh"
        ↓
Вводим в Invoice:
  - InvoiceNumber: "2024-001"
  - InvoiceDate: 2024-01-25
  - ClientId: 3 (ID записи в Client)
  - PersonenIndexEntryId: 18 (ID в Personen Index)
        ↓
Система читает PersonenIndexEntry #18:
  - UIDNumber: CHE123456789 ✓ (в счете)
  - CountryCode: CH ✓ (Швейцария!)
  - SuggestedIncomeAccount: 4001 ✓ (автоматически применяется)
        ↓
Система идет в EU-RATE.csv:
  - Ищет: CountryCode = "CH"
  - Находит: VAT Rate = 0% (Швейцария вне ЕС - реверс НДС)
        ↓
Расчет налога:
  - SubTotal: 2000€
  - Tax (0%): 0€ (реверс НДС)
  - Total: 2000€
  - Примечание: "Reverse VAT - Swiss customer"
```

---

## 📊 Таблица: Поля Personen Index и их использование

| Поле | Entity Свойство | Использование в ER | Использование в AR | Откуда берется |
|------|---|---|---|---|
| Kto-Nr | `KtoNr` | Идентификация | Идентификация | Входит в Excel |
| TAG | `TAG` | **Быстрый поиск** (вместо Kto-Nr) | **Быстрый поиск** | Входит в Excel (первые 5 букв) |
| Nachname | `CompanyName` | Печать счета | Печать счета | Входит в Excel |
| Land (Freifeld 01) | `CountryCode` | **Определяет налог в EU-RATE** | **Определяет налог в EU-RATE** | Входит в Excel |
| UID-Nummer | `UIDNumber` | На счете в печати | На счете в печати | Входит в Excel |
| Lief-Vorschlag | `SuggestedExpenseAccountId` | **Автоматически применяется** | — | Входит в Excel |
| Kunden-Vorschlag | `SuggestedIncomeAccountId` | — | **Автоматически применяется** | Входит в Excel |
| Filiale | `BranchNumber` | Опционально для филиалов | Опционально для филиалов | Входит в Excel |
| Land-Nr | `CountryNumber` | Сортировка | Сортировка | Входит в Excel |

---

## 🗂️ Как структурированы данные

### Personen Index (PersonenIndexEntry)
```sql
SELECT
  KtoNr, TAG, CompanyName, CountryCode, UIDNumber,
  SuggestedExpenseAccountId, SuggestedIncomeAccountId
FROM PersonenIndexEntries
WHERE Status = 'Active'
LIMIT 5;

-- Результат:
-- 300151  | MonoOst | Monolith Ost GmbH      | DE | DE123456789 | 5030 | NULL
-- 300234  | LogDat  | Logistik Daten AG      | AT | ATU987654   | 5030 | NULL
-- 200045  | AcmeCor | Acme Corp Gmbh         | CH | CHE111222   | NULL | 4001
-- 200089  | TechSys | Tech Systems Ltd       | GB | GB333444    | NULL | 4005
-- 400012  | BothCom | Both Commerce GmbH     | DE | DE555666    | 5030 | 4001
```

### EU-RATE (Справочник налогов)
```sql
SELECT
  Code, CountryCode, StandardRate,
  PurchaseThreshold, SupplyThreshold
FROM EuCountryData
WHERE Code IN ('DE', 'AT', 'CH');

-- Результат:
-- DE | DE | 19.0 | 100000 | 100000
-- AT | AT | 20.0 |  50000 |  50000
-- CH | CH |  0.0 |    N/A |    N/A  (вне ЕС)
```

### ER (ExpenseInvoice) + PersonenIndex
```sql
SELECT
  e.InvoiceNumber,
  e.InvoiceDate,
  p.TAG,
  p.CompanyName,
  p.CountryCode,
  e.SubTotal,
  e.TaxAmount,
  e.TotalAmount
FROM ExpenseInvoices e
LEFT JOIN PersonenIndexEntries p ON e.PersonenIndexEntryId = p.Id;

-- Результат:
-- 2024/1234 | 2024-01-20 | MonoOst  | Monolith Ost GmbH | DE | 1000 | 190 | 1190
-- 2024/1235 | 2024-01-21 | LogDat   | Logistik Daten AG | AT | 500  | 100 | 600
```

---

## 🎯 Ключевые концепции

### 1. **Уникальный источник правды (Single Source of Truth)**
Вся информация о контрагенте хранится **только в Personen Index**. ER/AR ссылаются на него, а не дублируют данные.

```
Нужно изменить адрес Monolith Ost?
  → Меняем в Personen Index
  → Все счета автоматически показывают новый адрес!
```

### 2. **Автоматическое подтягивание данных**
При вводе ER/AR система сама берет:
- Адрес, контакты из Personen Index
- Налог из EU-RATE на основе CountryCode
- Рекомендуемый счет из SuggestedAccountId

### 3. **Быстрый ввод через TAG**
Вместо ввода полного Kto-Nr (300151), вводим TAG (MonoOst):
- Быстрее печатать
- Меньше ошибок
- Человеко-ориентированный интерфейс

### 4. **Налоговый режим по стране**
```
CountryCode определяет все:
  - AT (Австрия, ЕС)      → Стандартный НДС 20%
  - DE (Германия, ЕС)     → Стандартный НДС 19%
  - CH (Швейцария, вне ЕС)→ Реверс НДС, 0% налог
  - US (США, вне ЕС)      → Реверс НДС, 0% налог
```

---

## 💾 Миграция и создание базы

```bash
# Создание миграции (уже выполнено)
dotnet ef migrations add PersonenIndexIntegration_ER_AR_Links \
  --startup-project src/QIMy.API \
  --project src/QIMy.Infrastructure

# Применение миграции
dotnet ef database update \
  --startup-project src/QIMy.API \
  --project src/QIMy.Infrastructure
```

---

## 📝 Пример использования в коде

### Получить данные контрагента при вводе счета (ER)

```csharp
// При вводе TAG (например, "MonoOst")
var personenIndexEntry = await context.PersonenIndexEntries
    .Include(p => p.SuggestedExpenseAccount)
    .Include(p => p.Country)
    .FirstOrDefaultAsync(p => p.TAG == "MonoOst");

// Автоматически заполняем:
var expenseInvoice = new ExpenseInvoice
{
    InvoiceNumber = "2024/1234",
    InvoiceDate = DateTime.UtcNow,
    SupplierId = supplier.Id,
    PersonenIndexEntryId = personenIndexEntry.Id,  // ✨ Ссылка!

    // Берем из Personen Index:
    // personenIndexEntry.UIDNumber
    // personenIndexEntry.CountryCode (для поиска в EU-RATE)
    // personenIndexEntry.SuggestedExpenseAccountId (автоматический счет)
};

// Определяем налог по стране
var vatRate = await context.EuCountryData
    .FirstOrDefaultAsync(r => r.CountryCode == personenIndexEntry.CountryCode);

expenseInvoice.TaxAmount = expenseInvoice.SubTotal * (vatRate.StandardRate / 100);
expenseInvoice.TotalAmount = expenseInvoice.SubTotal + expenseInvoice.TaxAmount;

await context.SaveChangesAsync();
```

---

## 🔍 Валидация и проверки

**При создании PersonenIndexEntry:**
- ✓ KtoNr должен быть уникальным
- ✓ TAG должен быть уникальным (для быстрого поиска)
- ✓ CountryCode должен быть действительным ISO кодом (AT, DE, CH...)
- ✓ UIDNumber должен соответствовать формату страны (ATU..., DE..., CHE...)
- ✓ Если ContractorType = Customer, то должен быть SuggestedIncomeAccountId
- ✓ Если ContractorType = Supplier, то должен быть SuggestedExpenseAccountId

**При создании ER/AR:**
- ✓ PersonenIndexEntryId должен указывать на существующую запись
- ✓ CountryCode из PersonenIndex должен быть в EU-RATE
- ✓ Налог должен пересчитываться при изменении суммы

---

## 📚 Связанные файлы

- Entity Models:
  - `src/QIMy.Core/Entities/PersonenIndexEntry.cs` (новая!)
  - `src/QIMy.Core/Entities/ExpenseInvoice.cs` (обновлена)
  - `src/QIMy.Core/Entities/Invoice.cs` (обновлена)

- Database:
  - `src/QIMy.Infrastructure/Data/ApplicationDbContext.cs` (обновлена)
  - Migration: `PersonenIndexIntegration_ER_AR_Links` (новая!)

- Services:
  - `src/QIMy.Infrastructure/Services/PersonenIndexImportService.cs` (импорт из Excel)

---

## 🚀 Что дальше?

1. **Импорт данных из Personen Index.xlsx**
   - Лист 6 (Länder) → PersonenIndexEntries
   - Лист 2 (EU-RATE) → EuCountryData

2. **Бизнес-логика в сервисах**
   - ExpenseInvoiceService: автоматическое подтягивание данных при вводе TAG
   - InvoiceService: определение налога по стране

3. **UI/API формы**
   - При вводе TAG → автозаполнение остальных полей
   - При выборе контрагента → расчет налога

4. **Отчеты и экспорт**
   - VAT отчеты по странам (на основе CountryCode)
   - Реестр контрагентов с их статусом

---

**Автор**: AI Assistant
**Дата**: 2026-01-24
**Версия**: 1.0

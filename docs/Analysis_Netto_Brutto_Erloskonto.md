# АНАЛИЗ: Система цен и НДС (Netto/Brutto + Erlöskonto)

## Текущая ситуация

### Старый QIM (Entity Framework, SQL Server)
**Структура данных:**
- **TaxRate**: Ставка НДС (0%, 10%, 13%, 20%)
- **Account**: Счет доходов/Erlöskonto (4010, 4013, 4020, 4030, 4062, 4100, 4112, и т.д.)
- **Tax** = TaxRate + Account (связывает ставку НДС со счетом доходов)
- **InvoiceItem**:
  - UnitPrice = NETTO цена (без НДС)
  - TaxID → Tax → TaxRate.Rate + Account.AccountNumber
  - Brutto = Netto * (1 + TaxRate/100)

**Формулы в коде:**
```csharp
// Invoice.cs (PartialClasses.cs)
public decimal? Total => InvoiceItems?.Sum(ii => ii.Quantity * ii.UnitPrice * (ii.Tax?.TaxRate?.Rate / 100 + 1));
public decimal? TaxAmount => InvoiceItems?.Sum(ii => ii.Quantity * ii.UnitPrice * (ii.Tax?.TaxRate?.Rate / 100));
```

**SQL запросы:**
```sql
-- Netto сумма
sum(II.Quantity * II.UnitPrice)

-- НДС
sum(II.Quantity * II.UnitPrice * TR.Rate / 100)

-- Brutto (Total)
sum(II.Quantity * II.UnitPrice) + sum(II.Quantity * II.UnitPrice * TR.Rate / 100)

-- Счет доходов
A.AccountNumber (из Accounts через Tax)
```

**Инициализация базовых счетов:**
```sql
insert into Accounts(ClientAreaID, AccountNumber, AccountCode, IsForServices)
values
    (@DomesticClientAreaID, N'4010', N'1', 0),
    (@DomesticClientAreaID, N'4013', N'1', 0),
    (@DomesticClientAreaID, N'4020', N'1', 0),
    (@DomesticClientAreaID, N'4030', N'1', 0);
```

### Новый QIMy (EF Core, Blazor Server)
**Текущая структура:**
- **TaxRate**: Ставка НДС (Rate: decimal)
- **InvoiceItem**:
  - UnitPrice = цена (NETTO?)
  - TaxRate = ставка НДС (число)
  - TaxAmount = сумма НДС
  - TotalAmount = общая сумма (BRUTTO?)

**Проблемы:**
1. ❌ Нет сущности Account (счет доходов/Erlöskonto)
2. ❌ Нет связи между TaxRate и счетом доходов
3. ❌ Непонятно, какая цена в UnitPrice (netto или brutto?)
4. ❌ TaxAmount и TotalAmount могут рассинхронизироваться

## Требования пользователя

> "цена должна указываться в нетто и брутто, также должна стоять ставка НДС, в зависимости от Erlöskonto (4000,4062,4112,4100)"

**Интерпретация:**
1. При создании счета (Invoice) нужно **явно показывать** Netto и Brutto цены
2. Ставка НДС должна **автоматически определяться** по счету доходов (Erlöskonto)
3. Счета доходов: 4000, 4010-4018, 4030, 4062, 4100, 4112, 4850

**Типичные связки (Австрия):**
- **4000, 4030** → 20% (стандартная ставка)
- **4062** → 10% (сниженная ставка)
- **4100** → 0% (экспорт, освобожденный от НДС)
- **4112** → 13% (специальная ставка)

## Предлагаемое решение

### Вариант 1: Добавить сущность Account (как в старом QIM)

```csharp
// QIMy.Core/Entities/Account.cs
public class Account : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty; // 4000, 4062, 4100, 4112
    public string AccountCode { get; set; } = string.Empty;   // Код налогового кода (1, 2, 3)
    public int ClientAreaId { get; set; }                     // Inland, EU, Export
    public bool IsForServices { get; set; } = false;          // Для услуг или товаров
    
    // Navigation
    public ClientArea? ClientArea { get; set; }
}

// QIMy.Core/Entities/Tax.cs (новая)
public class Tax : BaseEntity
{
    public int TaxRateId { get; set; }
    public int AccountId { get; set; }
    
    // Navigation
    public TaxRate TaxRate { get; set; } = null!;
    public Account Account { get; set; } = null!;
}

// Обновить InvoiceItem
public class InvoiceItem : BaseEntity
{
    // ...
    public int TaxId { get; set; }  // Вместо decimal TaxRate
    
    // Navigation
    public Tax Tax { get; set; } = null!;
}
```

**Преимущества:**
✅ Полная совместимость со старым QIM
✅ Явная связь: Erlöskonto → TaxRate
✅ Легко мигрировать данные из старой системы

**Недостатки:**
❌ Более сложная модель данных
❌ Нужно создать миграции и пересоздать Invoice/InvoiceItem
❌ Нужно добавить UI для управления Accounts и Taxes

### Вариант 2: Добавить поле RevenueAccount в InvoiceItem (упрощенный)

```csharp
public class InvoiceItem : BaseEntity
{
    // Существующие поля
    public decimal UnitPrice { get; set; }        // NETTO цена
    public decimal TaxRate { get; set; }          // Ставка НДС (в процентах)
    public decimal TaxAmount { get; set; }        // Сумма НДС (вычисляется)
    public decimal TotalAmount { get; set; }      // BRUTTO (вычисляется)
    
    // Новое поле
    public string? RevenueAccount { get; set; }   // Erlöскonto: 4000, 4062, 4100, 4112
    
    // Вычисляемое свойство
    [NotMapped]
    public decimal NettoAmount => Quantity * UnitPrice;
    
    [NotMapped]
    public decimal BruttoAmount => NettoAmount + TaxAmount;
}

// Логика при создании/редактировании InvoiceItem:
public static decimal GetTaxRateByAccount(string revenueAccount)
{
    return revenueAccount switch
    {
        "4000" or "4030" => 20m,
        "4062" => 10m,
        "4100" => 0m,
        "4112" => 13m,
        _ => 20m // default
    };
}
```

**Преимущества:**
✅ Простая реализация
✅ Не нужно переделывать всю модель данных
✅ Легко добавить через миграцию

**Недостатки:**
❌ Менее гибкая система
❌ Логика связи Account→TaxRate жестко закодирована
❌ Сложнее поддерживать при изменении налоговых ставок

### Вариант 3: Hybrid - добавить Account, но упростить Tax

```csharp
// QIMy.Core/Entities/Account.cs
public class Account : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty; // 4000, 4062, etc.
    public string Name { get; set; } = string.Empty;          // Описание
    public int DefaultTaxRateId { get; set; }                 // Ссылка на TaxRate
    public int ClientAreaId { get; set; }                     // Inland, EU, Export
    
    // Navigation
    public TaxRate DefaultTaxRate { get; set; } = null!;
    public ClientArea? ClientArea { get; set; }
}

// InvoiceItem остается как есть, но добавляем Account
public class InvoiceItem : BaseEntity
{
    // ...
    public int? AccountId { get; set; }           // Ссылка на счет доходов
    
    // Navigation
    public Account? Account { get; set; }
}

// При создании InvoiceItem:
// 1. Выбираем Account
// 2. TaxRate автоматически берется из Account.DefaultTaxRate
// 3. TaxAmount и TotalAmount вычисляются автоматически
```

**Преимущества:**
✅ Баланс между гибкостью и простотой
✅ Легко управлять счетами доходов через UI
✅ Можно изменять TaxRate для Account без изменения кода

**Недостатки:**
❌ Всё равно нужны миграции
❌ Нужен UI для управления Accounts

## Рекомендация

**Выбрать Вариант 3 (Hybrid)**:
1. Добавить сущность Account с полями AccountNumber и DefaultTaxRateId
2. Добавить AccountId в InvoiceItem
3. При создании InvoiceItem автоматически заполнять TaxRate из Account.DefaultTaxRate.Rate
4. Вычислять TaxAmount и TotalAmount автоматически

**План реализации:**
1. Создать миграцию для Account
2. Создать сервис для управления Accounts
3. Обновить UI создания счета:
   - Добавить выбор Erlöскonto (Account)
   - Показывать Netto и Brutto цены
   - Автоматически обновлять TaxRate при выборе Account
4. Добавить seed данных для базовых счетов (4000, 4062, 4100, 4112, 4030)

**Время реализации:** 2-3 часа

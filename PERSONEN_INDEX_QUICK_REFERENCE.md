# ⚡ QUICK START: Personen Index Архитектура

## 🎯 В одном предложении
**Personen Index** = центральный реестр всех контрагентов; **ER/AR** = счета, которые ссылаются на реестр; **EU-RATE** = налоги по странам.

---

## 🏛️ 3 основные таблицы

| Таблица | Роль | Пример |
|---------|------|--------|
| **PersonenIndexEntries** | 🧠 Мозг системы | Monolith Ost: TAG=MonoOst, CountryCode=DE |
| **ExpenseInvoices (ER)** | 💰 Входящие | Счет от Monolith, ссылается на PersonenIndex |
| **Invoices (AR)** | 📄 Исходящие | Счет клиенту, ссылается на PersonenIndex |

---

## 🚀 Как работает

### Входящий счет (ER)
```
1. Приходит счет от поставщика
2. Вводим TAG (вместо кода) → "MonoOst"
3. Система находит в Personen Index
4. Подтягивает: страну (DE), налог (19%), рекомендуемый счет (5030)
5. Автоматически считает сумму с налогом
```

### Исходящий счет (AR)
```
1. Выставляем счет клиенту
2. Вводим TAG → "AcmeCor"
3. Система находит в Personen Index
4. Видит страну (CH) → применяет реверс НДС (0%)
5. Автоматически считает сумму БЕЗ налога
```

---

## 📊 Структура PersonenIndexEntry

```csharp
public class PersonenIndexEntry
{
    public string KtoNr { get; set; }                  // 300151 (поставщик)
    public string TAG { get; set; }                    // MonoOst (5 букв для ввода)
    public string CompanyName { get; set; }            // Monolith Ost GmbH
    public string CountryCode { get; set; }            // DE (определяет налог!)
    public string UIDNumber { get; set; }              // DE123456789 (для документов)
    public int? SuggestedExpenseAccountId { get; set; }// 5030 (для ER)
    public int? SuggestedIncomeAccountId { get; set; } // 4001 (для AR)
    public ContractorType ContractorType { get; set; }// Customer/Supplier/Both
    public ContractorStatus Status { get; set; }       // Active/Inactive/Pending/Blocked
}
```

---

## 🔗 Связь таблиц

```
PersonenIndexEntry (1)
         ↓ ↓
        ER AR
        ↓  ↓
    ExpenseInvoice + Invoice
```

```csharp
// В ExpenseInvoice (ER):
public int? PersonenIndexEntryId { get; set; }        // ← Ссылка на реестр

// В Invoice (AR):
public int? PersonenIndexEntryId { get; set; }        // ← Ссылка на реестр
```

---

## 📋 Kto-Nr кодирование

| Код | Тип | Роль | Пример |
|-----|-----|------|--------|
| **2xxxxx** | Customer | AR только | 200045 = Acme Corp |
| **3xxxxx** | Supplier | ER только | 300151 = Monolith Ost |
| **4xxxxx** | Both | AR + ER | 400012 = Both Commerce |

---

## 🌍 Налоги по странам

```
CountryCode → EU-RATE → Налоговая ставка

AT (Австрия) → 20% НДС
DE (Германия) → 19% НДС
CH (Швейцария) → 0% НДС (реверс)
US (США) → 0% НДС (реверс)
```

---

## 💻 Быстрый поиск (TAG)

```csharp
// Вместо этого (забудут код):
var supplier = await context.Suppliers
    .FirstOrDefaultAsync(s => s.Code == "300151");

// Пишем так (легко помнить):
var contractor = await context.PersonenIndexEntries
    .FirstOrDefaultAsync(p => p.TAG == "MonoOst");
```

---

## ✅ Что было сделано

- ✅ Создана Entity `PersonenIndexEntry`
- ✅ Обновлена `ExpenseInvoice` - добавлена ссылка на PersonenIndex
- ✅ Обновлена `Invoice` - добавлена ссылка на PersonenIndex
- ✅ Обновлена `ApplicationDbContext` - добавлен DbSet
- ✅ Создана и применена миграция
- ✅ Проект успешно собран (0 ошибок)

---

## 🚀 Что дальше?

1. **Импорт данных** - загрузить контрагентов из Personen Index.xlsx
2. **API endpoints** - создать CRUD операции для PersonenIndexEntry
3. **UI формы** - добавить автозаполнение при вводе TAG
4. **Валидация** - проверка уникальности KtoNr/TAG

---

## 📚 Документация

- **Полная архитектура:** [docs/PERSONEN_INDEX_ER_AR_ARCHITECTURE.md](docs/PERSONEN_INDEX_ER_AR_ARCHITECTURE.md)
- **Примеры кода:** [src/QIMy.Application/Examples/PersonenIndexUsageExamples.cs](src/QIMy.Application/Examples/PersonenIndexUsageExamples.cs)
- **Статус реализации:** [PERSONEN_INDEX_IMPLEMENTATION_COMPLETE.md](PERSONEN_INDEX_IMPLEMENTATION_COMPLETE.md)

---

## 🎓 Ключевой момент

> **Personen Index = Single Source of Truth (SSoT)**
> 
> Все данные контрагента хранятся в ЭТО месте. Изменяешь адрес в Personen Index → все счета показывают новый адрес. Просто и надежно!

---

**Версия:** 1.0  
**Дата:** 2026-01-24  
**Статус:** ✅ ГОТОВО

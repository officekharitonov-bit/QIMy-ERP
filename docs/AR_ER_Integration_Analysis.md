# 📊 Анализ взаимодействия AR (Исходящие счета) и ER (Входящие счета)

**Дата**: 24 января 2026  
**Источники**: Анализ старого QIM, структура базы данных, Personen Index, Products CSV

---

## 🎯 Основная концепция

QIMy - это система управления счетами и документооборотом с двумя основными модулями:

### AR (Ausgangsrechnungen) - Исходящие счета
- **Пользователь**: Продавец (компания)
- **Партнер**: Клиент (покупатель)
- **Документ**: Счет на оплату (Invoice)
- **Назначение**: Продажи товаров/услуг
- **Результат**: Доход (Revenue)

### ER (Eingangsrechnungen) - Входящие счета
- **Пользователь**: Покупатель (компания)
- **Партнер**: Поставщик (продавец)
- **Документ**: Счет от поставщика (ExpenseInvoice)
- **Назначение**: Закупки товаров/услуг
- **Результат**: Расход (Expense)

---

## 🔗 Точки связи между AR и ER

### 1. **Справочники (Reference Data) - ОБЩИЕ для AR и ER**

```
┌─────────────────────────────────────────────────────┐
│                  СПРАВОЧНИКИ                         │
├─────────────────────────────────────────────────────┤
│                                                       │
│  Product (Товар/Услуга)                              │
│  ├─ Name, SKU, PartNumber, Brand                    │
│  ├─ Price (нетто)                                   │
│  ├─ UnitId (ссылка на Unit)                         │
│  └─ TaxRateId (ссылка на TaxRate)                   │
│                                                       │
│  Unit (Единица измерения)                           │
│  ├─ Std (час работы)                                │
│  ├─ Stück (штука)                                   │
│  ├─ Pausch (паушаль)                                │
│  └─ Abr. (абонентская плата)                       │
│                                                       │
│  TaxRate (Налоговая ставка)                         │
│  ├─ 20% (стандартная ставка)                       │
│  ├─ 10% (льготная)                                 │
│  └─ 0% (экспорт)                                    │
│                                                       │
│  Account (Счет доходов/расходов)                    │
│  ├─ 4000: Umsatzerlöse 20% (Доход 20%)             │
│  ├─ 4010: Umsatzerlöse 10% (Доход 10%)             │
│  ├─ 4020: Umsatzerlöse 0% (Доход 0%)               │
│  ├─ 4030: Innergemeinschaftliche Lieferungen       │
│  ├─ 4100: Erlöse Ausland (Экспорт)                 │
│  └─ [и другие счета расходов]                      │
│                                                       │
│  Currency (Валюта)                                  │
│  ├─ EUR (евро)                                      │
│  ├─ USD (доллар)                                    │
│  └─ RUB (рубль)                                     │
│                                                       │
│  PaymentMethod (Способ оплаты)                      │
│  ├─ Bank Transfer                                   │
│  ├─ Cash                                            │
│  └─ Credit Card                                     │
│                                                       │
└─────────────────────────────────────────────────────┘
```

**Назначение**: Все справочники ОБЩИЕ для AR и ER, чтобы обеспечить:
- Однозначное определение налогов
- Единую нумерацию в бухгалтерии
- Консистентность валют и операций
-统一的 Product Catalog

---

### 2. **Партнеры - РАЗНЫЕ справочники**

```
┌─────────────────────────────────────────────────────┐
│        AR (ИСХОДЯЩИЕ СЧЕТА)                          │
├─────────────────────────────────────────────────────┤
│                                                       │
│  Client (Клиент)                                    │
│  ├─ ClientCode: 200000-299999                       │
│  ├─ CompanyName (название компании)                │
│  ├─ Address, City, PostalCode, Country             │
│  ├─ Email, Phone, Website                          │
│  ├─ VatNumber (НДС номер)                          │
│  ├─ IBAN, BIC (банковские реквизиты)               │
│  ├─ PaymentTermsDays (условия оплаты в днях)      │
│  ├─ CreditLimit (кредитный лимит)                  │
│  ├─ DefaultPaymentMethodId (способ оплаты)         │
│  ├─ ClientTypeId (B2B/B2C) → ClientType            │
│  ├─ ClientAreaId (Inland/EU/Drittland) → Area     │
│  └─ Invoices (One-to-Many связь)                   │
│                                                       │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│        ER (ВХОДЯЩИЕ СЧЕТА)                          │
├─────────────────────────────────────────────────────┤
│                                                       │
│  Supplier (Поставщик)                               │
│  ├─ SupplierCode: 300000-399999                     │
│  ├─ CompanyName (название компании)                │
│  ├─ Address, City, PostalCode, Country             │
│  ├─ Email, Phone, Website                          │
│  ├─ VatNumber (НДС номер)                          │
│  ├─ IBAN, BIC (банковские реквизиты)               │
│  ├─ PaymentTermsDays (условия оплаты)              │
│  ├─ SupplierAreaId (Inland/EU/Drittland)           │
│  └─ ExpenseInvoices (One-to-Many связь)            │
│                                                       │
└─────────────────────────────────────────────────────┘
```

**Отличие**: 
- Разные диапазоны кодов (200k для клиентов, 300k для поставщиков)
- Практически идентичная структура данных
- Разные типы связанных документов (Invoice vs ExpenseInvoice)

---

### 3. **Основные документы - ПАРАЛЛЕЛЬНЫЕ структуры**

```
AR (ИСХОДЯЩИЕ) ←→ ER (ВХОДЯЩИЕ)

Invoice (Исходящий счет)          ←→  ExpenseInvoice (Входящий счет)
├─ InvoiceNumber                      ├─ InvoiceNumber (от поставщика)
├─ ClientId → Client                 ├─ SupplierId → Supplier
├─ IssueDate                          ├─ IssueDate
├─ DueDate                            ├─ DueDate
├─ SubTotal (Netto)                   ├─ SubTotal (Netto)
├─ TaxAmount (НДС)                    ├─ TaxAmount (НДС)
├─ TotalAmount (Brutto)               ├─ TotalAmount (Brutto)
├─ PaidAmount                         ├─ PaidAmount
├─ Status (Draft/Sent/Paid/...)       ├─ Status (Draft/Received/Approved/...)
├─ CurrencyId                         ├─ CurrencyId
├─ BankAccountId                      ├─ [Способ оплаты не явный]
├─ PaymentMethodId                    ├─ [Выбирается при платеже]
├─ Terms (текст условий)              ├─ Terms (текст условий)
├─ Notes                              ├─ Notes
└─ Items (позиции счета)              └─ Items (позиции счета)
   ├─ InvoiceItem                         ├─ ExpenseInvoiceItem
   ├─ Description                        ├─ Description
   ├─ Quantity                           ├─ Quantity
   ├─ UnitPrice (Netto)                  ├─ UnitPrice (Netto)
   ├─ UnitId                             ├─ [обычно не заполняется]
   ├─ TaxId (связь Tax.TaxRate)          ├─ TaxRate (процент, не FK)
   ├─ ProductId (опционально)            ├─ [обычно не заполняется]
   └─ Calculated TotalAmount             └─ Calculated TotalAmount
```

**Ключевое отличие в TaxRate**:
- **Invoice**: использует Foreign Key `TaxId` → `Tax` → `TaxRate` (жесткая связь)
- **ExpenseInvoice**: хранит `TaxRate` как decimal процент (гибкость)

---

## 📈 Бизнес-процесс и данные-движения

### 1. **Поступление товара/услуги**

```
ПОСТАВЩИК (Supplier)
       ↓
   отправляет
       ↓
ВХОДЯЩИЙ СЧЕТ (ExpenseInvoice)
       ↓
   содержит позиции
       ↓
ПОЗИЦИИ СЧЕТА (ExpenseInvoiceItem)
       ↓
   со ставками НДС и суммами
       ↓
БУХГАЛТЕРСКАЯ ЗАПИСЬ (AccountingEntry)
├─ Дебет: 6xxx (Материалы/Услуги)
└─ Кредит: 2xxx (Кредиторы)
```

### 2. **Отправка счета клиенту**

```
КЛИЕНТ (Client)
       ↓
   получит счет из
       ↓
ИСХОДЯЩИЙ СЧЕТ (Invoice)
       ↓
   содержит позиции
       ↓
ПОЗИЦИИ СЧЕТА (InvoiceItem)
       ↓
   со ставками НДС и суммами
       ↓
БУХГАЛТЕРСКАЯ ЗАПИСЬ (AccountingEntry)
├─ Дебет: 1xxx (Дебиторы)
└─ Кредит: 4xxx (Доход - Erlöskonto)
```

### 3. **Получение платежа**

```
ПЛАТЕЖ ОТ КЛИЕНТА (Payment)
       ↓
   относится к Invoice
       ↓
   уменьшает PaidAmount
       ↓
   статус Invoice изменяется
       ├─ Sent → если полная оплата
       ├─ Paid → если полная оплата
       └─ PartiallyPaid → если частичная
       ↓
   возникает AccountingEntry
   ├─ Дебет: Банк/Касса
   └─ Кредит: Дебиторы
```

### 4. **Оплата счета поставщика**

```
ПЛАТЕЖ ПОСТАВЩИКУ (SupplierPayment)
       ↓
   относится к ExpenseInvoice
       ↓
   уменьшает PaidAmount
       ↓
   статус ExpenseInvoice изменяется
       ├─ Received → если полная оплата
       ├─ Paid → если полная оплата
       └─ PartiallyPaid → если частичная
       ↓
   возникает AccountingEntry
   ├─ Дебет: Кредиторы
   └─ Кредит: Банк/Касса
```

---

## 🎯 Роль справочников в AR и ER

### Product (Товар/Услуга) - основной справочник

```
┌───────────────────────────────────────────┐
│  PRODUCT в системе                        │
├───────────────────────────────────────────┤
│                                            │
│  Используется в:                          │
│  1. InvoiceItem (AR):                     │
│     - Заполнение описания позиции         │
│     - Подтягивание цены                   │
│     - Подтягивание налога по умолчанию    │
│                                            │
│  2. ExpenseInvoiceItem (ER):              │
│     - РЕДКО используется (обычно None)    │
│     - Если заполнено: для учета затрат    │
│                                            │
│  3. Stock Management:                     │
│     - Уменьшение наличия (AR)             │
│     - Увеличение наличия (ER)             │
│                                            │
│  Fields:                                  │
│  ├─ Name: "Beratung" (Консультация)      │
│  ├─ SKU: "5"                              │
│  ├─ Brand: (обычно пусто)                 │
│  ├─ Price: 100.00 (NETTO EUR)             │
│  ├─ UnitId → "Std" (часы)                 │
│  ├─ TaxRateId → 20%                       │
│  ├─ IsService: true                       │
│  ├─ StockQuantity: 0 (услуги)             │
│  └─ Notes: (дополнительно)                │
│                                            │
└───────────────────────────────────────────┘

Примеры товаров из CSV:
1: "BH lfd." - Std, 75.00 EUR, 20%
2: "JA Std." - Std, 100.00 EUR, 20%
5: "Beratung" - Std, 100.00 EUR, 20%
21: "BILANZ (klein)" - Pausch, 1050.00 EUR, 20%
35: "Weiterverechnete Spesen" - Pausch, 0.00 EUR, 0%
36: "Fahrt" - km, 0.20 EUR, 10%
```

### TaxRate (Налоговая ставка) - обязательный справочник

```
┌───────────────────────────────────────────┐
│  TAXRATE в AR и ER                        │
├───────────────────────────────────────────┤
│                                            │
│  Значения по умолчанию:                  │
│  ├─ 20% (стандартная для Austria)        │
│  ├─ 10% (льготная для товаров питания)   │
│  ├─ 13% (другие услуги)                  │
│  └─ 0% (экспорт)                         │
│                                            │
│  Использование:                           │
│  1. В InvoiceItem (AR):                   │
│     - Product.TaxRateId подтягивает значение
│     - Может быть переопределено вручную   │
│     - Используется для расчета НДС        │
│                                            │
│  2. В ExpenseInvoiceItem (ER):            │
│     - Хранится как простой процент        │
│     - Вводится вручную (часто)            │
│     - Может не совпадать с Product        │
│                                            │
│  3. В Account:                            │
│     - Account 4000 связан с TaxRate 20%  │
│     - Account 4010 связан с TaxRate 10%  │
│     - Account 4100 связан с TaxRate 0%   │
│                                            │
└───────────────────────────────────────────┘
```

### Account (Счета в бухгалтерии) - обязательный справочник

```
┌───────────────────────────────────────────┐
│  ACCOUNT в AR и ER                        │
├───────────────────────────────────────────┤
│                                            │
│  ДОХОД (для AR - Invoice):                │
│  ├─ 4000: "Umsatzerlöse 20% USt"         │
│  ├─ 4010: "Umsatzerlöse 10% USt"         │
│  ├─ 4020: "Umsatzerlöse 0% USt"          │
│  ├─ 4030: "Innergemeinschaftl. Lief." (0%)
│  ├─ 4100: "Erlöse Ausland" (Экспорт)     │
│  └─ 4850: "Sonstige betriebliche Erträge"│
│                                            │
│  РАСХОД (для ER - ExpenseInvoice):        │
│  ├─ 6010: "Rohstoffe" (Материалы)        │
│  ├─ 6020: "Kaufteile" (Покупные части)   │
│  ├─ 6100: "Merch/Waren" (Товары)         │
│  ├─ 6200: "Aufzuwendungen" (Услуги)      │
│  ├─ 6240: "Dienstleistungen Fremde"      │
│  ├─ 6300: "Verschleißteile" (Расходники) │
│  ├─ 7100: "Leasing" (Лизинг)             │
│  └─ [и другие]                           │
│                                            │
│  Связь с Products:                        │
│  - Invoice использует Account через Tax   │
│  - Tax связывает Account и TaxRate        │
│  - Product.TaxRateId → Tax → Account      │
│                                            │
│  Пример маршрута для Invoice:             │
│  Product "Beratung" (100 EUR, 20%)        │
│  └─ TaxRate 20%                           │
│     └─ Account 4000 (Umsatzerlöse 20%)   │
│        └─ Бухгалтерия: Кредит счета 4000│
│           Сумма: 100 EUR + 20 EUR НДС    │
│                                            │
└───────────────────────────────────────────┘
```

---

## 🔍 Пример: Как работает один счет (Invoice) с справочниками

### Сценарий: Счет от консалтинговой компании

```
1. КЛИЕНТ (из Client таблицы):
   ├─ ClientCode: 200015
   ├─ CompanyName: "ANDREI GIGI"
   ├─ Country: Austria
   ├─ VatNumber: (пусто)
   ├─ DefaultPaymentMethodId: (пусто)
   └─ CurrencyId: EUR

2. СЧЕТ (Invoice):
   ├─ InvoiceNumber: "INV-20260124-001"
   ├─ ClientId: 200015
   ├─ IssueDate: 2026-01-24
   ├─ DueDate: 2026-02-24 (30 дней)
   ├─ CurrencyId: EUR
   └─ Status: Draft

3. ПОЗИЦИЯ 1 - КОНСУЛЬТАЦИЯ:
   ├─ Description: "Консультация по налогам"
   ├─ ProductId: 5 (из Product таблицы)
   │  └─ Product.Name: "Beratung"
   │  └─ Product.Price: 100.00
   │  └─ Product.UnitId: 2 (Std - час)
   │  └─ Product.TaxRateId: 1 (20%)
   ├─ Quantity: 5 (часов)
   ├─ UnitPrice: 100.00 (NETTO)
   │  └─ [подтянуто из Product.Price]
   ├─ TaxId: 1
   │  └─ Tax.TaxRateId: 1 (20%)
   │     └─ Tax.AccountId: 4000 (Umsatzerlöse 20%)
   ├─ NettoAmount: 5 * 100 = 500 EUR
   ├─ TaxAmount: 500 * 20% = 100 EUR
   └─ BruttoAmount: 500 + 100 = 600 EUR

4. ПОЗИЦИЯ 2 - ПЕРЕПРАВЛЕНИЯ:
   ├─ Description: "Бензин и переправления"
   ├─ ProductId: 35 (из Product таблицы)
   │  └─ Product.Name: "Weiterverechnete Spesen"
   │  └─ Product.Price: 0.00
   │  └─ Product.TaxRateId: NULL (0%)
   ├─ Quantity: 1
   ├─ UnitPrice: 75.00 (NETTO) [вручную]
   ├─ TaxId: NULL
   │  └─ Может быть TaxRate 0% или не выбран
   ├─ NettoAmount: 1 * 75 = 75 EUR
   ├─ TaxAmount: 0 EUR
   └─ BruttoAmount: 75 EUR

5. ИТОГИ СЧЕТА:
   ├─ SubTotal: 500 + 75 = 575 EUR (NETTO)
   ├─ TaxAmount: 100 + 0 = 100 EUR (НДС)
   ├─ TotalAmount: 575 + 100 = 675 EUR (BRUTTO)
   ├─ PaidAmount: 0 EUR
   └─ Balance: 675 EUR

6. БУХГАЛТЕРСКИЕ ЗАПИСИ (при сохранении):
   ├─ Дебет: 1100 (Дебиторы/Клиенты) 675 EUR
   ├─ Кредит: 4000 (Доход 20%) 500 EUR + 100 EUR НДС
   └─ Кредит: 4020 (Доход 0%) 75 EUR

7. СТАТУСЫ В ТЕЧЕНИЕ ВРЕМЕНИ:
   ├─ Draft (2026-01-24) - создано
   ├─ Sent (2026-01-25) - отправлено клиенту
   ├─ Paid (2026-02-24) - получена полная оплата
   └─ Archived (позже)

8. ПЛАТЕЖ (Payment):
   ├─ Amount: 675 EUR
   ├─ Date: 2026-02-20
   ├─ PaymentMethod: Bank Transfer
   └─ Invoice.PaidAmount: 675 EUR
   └─ Invoice.Status: Paid
```

---

## 📋 Сравнительная таблица AR vs ER

| Аспект | AR (Исходящие) | ER (Входящие) | Общее |
|--------|---|---|---|
| **Документ** | Invoice | ExpenseInvoice | Оба содержат Items |
| **Партнер** | Client (200k) | Supplier (300k) | Разные справочники |
| **Цель** | Продажа | Закупка | Взаимнодополняющие |
| **Статус** | Draft/Sent/Paid | Draft/Received/Approved/Paid | Похожие статусы |
| **Справочники** | Общие (Product, TaxRate, Account, Currency) | Общие | КРИТИЧНО! |
| **TaxRate** | FK через Tax | Простой decimal | Разные подходы |
| **ProductId** | Часто заполнен | Редко заполнен | Опциональный |
| **Payment** | От клиента | Поставщику | Разные направления |
| **Отчеты** | Revenue Report | Expense Report | Итоговый P&L |

---

## 🎓 Выводы для QIMy

### 1. **ОБЩИЕ справочники - КРИТИЧНО для согласованности**
- Product, TaxRate, Unit, Currency, Account должны быть ЕДИНЫЕ
- Изменение ставки НДС должно отражаться везде автоматически
- Product.Price должен быть консистентен между AR и ER

### 2. **Структура документов - ПАРАЛЛЕЛЬНАЯ, но разная**
- Invoice и ExpenseInvoice должны иметь почти идентичные поля
- Но разные связанные Entity (Client vs Supplier)
- Разные статусы и процессы согласования

### 3. **TaxRate в Item - РАЗЛИЧИЕ в подходах**
- InvoiceItem использует TaxId (FK) → гарантирует консистентность
- ExpenseInvoiceItem использует TaxRate (decimal) → гибкость
- Рекомендация: сделать одинаковым (используя TaxId везде)

### 4. **Product.TaxRateId - СВЯЗЬ между справочниками**
- Product должен иметь DefaultTaxRate
- При создании Item, TaxRate подтягивается из Product
- Но может быть переопределено вручную

### 5. **Бухгалтерия - ОСНОВНАЯ ценность**
- Справочник Account определяет счета в бухгалтерии
- TaxRate + Account = правильный счет для записи
- Без правильного Account невозможно вести бухгалтерию

### 6. **Интеграция данных - ПУТЬ ДВИЖЕНИЯ**
```
Client/Supplier 
  ↓
Invoice/ExpenseInvoice
  ↓
Item (InvoiceItem/ExpenseInvoiceItem)
  ↓
Product (опционально, но часто)
  ↓
TaxRate (обязательно)
  ↓
Account (обязательно для бухгалтерии)
```

---

## 🚀 План реализации в QIMy

### Phase 1: Основная структура (ГОТОВО в текущем спринте)
- ✅ Client с расширенными полями
- ✅ Product с новыми полями (Brand, PartNumber, etc)
- ✅ Invoice с позициями
- ✅ Tax/TaxRate связи
- ✅ Product catalog

### Phase 2: Входящие счета (ER) - СЛЕДУЮЩИЙ спринт
- 📋 Supplier (копия структуры Client но с кодами 300k)
- 📋 ExpenseInvoice структура
- 📋 ExpenseInvoiceItem структура
- 📋 Связь с Account для расходов

### Phase 3: Платежи и отчеты - ЧЕРЕЗ СПРИНТ
- 📋 Payment система для обоих (AR и ER)
- 📋 Бухгалтерские записи (AccountingEntry)
- 📋 Отчеты (Revenue, Expense, P&L)

### Phase 4: Интеграция - ФИНАЛЬНЫЙ спринт
- 📋 Согласование валют между AR и ER
- 📋 Reconciliation (сопоставление платежей)
- 📋 Multi-company/Multi-branch поддержка

---

## 📚 Ссылки на документацию

- [Personen_Index_Structure.md](./Personen_Index_Structure.md) - Структура импорта клиентов
- [COMPLETE_OLD_QIM_STRUCTURE.md](../COMPLETE_OLD_QIM_STRUCTURE.md) - Полная архитектура старого QIM
- [ANALYSIS_OLD_QIM.md](../ANALYSIS_OLD_QIM.md) - Анализ паттернов старого QIM

---

**Автор**: AI Assistant  
**Дата обновления**: 24 января 2026  
**Версия**: 1.0

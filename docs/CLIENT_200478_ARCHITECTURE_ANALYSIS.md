# 🏛️ IT-АРХИТЕКТУРНЫЙ АНАЛИЗ: Структура данных клиента 200478

**Дата анализа:** 24 января 2026
**Клиент:** 200478
**Архитектор:** AI Assistant
**Уровень анализа:** Enterprise Integration Pattern

---

## 📊 ОБЗОР АРХИТЕКТУРЫ

### Структура клиента (200478) - это пример **Enterprise Data Warehouse** паттерна

```
┌────────────────────────────────────────────────────────────────┐
│                    GOOGLE CLOUD (клиент)                       │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Personen     │  │  1_AR        │  │  2_ER        │          │
│  │ Index        │  │  (Invoices)  │  │  (Expenses)  │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐                            │
│  │  3_BANK      │  │  4_KASSA     │                            │
│  │  (Statements)│  │  (Cash Box)  │                            │
│  └──────────────┘  └──────────────┘                            │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  BUCHUNGSSCHRITTE (Booking Steps / Ledger Entries)      │  │
│  │  ⚠️ Внимание: Это ПЕРЕСЫЛКА РАСЧЕТОВ!                    │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
                            ↓
                   (ВЫГРУЗКА ДАННЫХ)
                            ↓
┌────────────────────────────────────────────────────────────────┐
│           BMD NTCS (Бухгалтерская система)                     │
│  • Корректировка и обработка                                   │
│  • Генерирование отчетов                                       │
│  • НДС отчеты                                                  │
│  • Финансовые результаты                                       │
└────────────────────────────────────────────────────────────────┘
```

---

## 🗂️ СТРУКТУРА ПАПОК (ЛОГИЧЕСКИЙ ДИЗАЙН)

### 1. **Personen Index** (Справочник контрагентов)
```
📄 Personen index.xlsx
   ├─ Kto-Nr: Уникальный номер счета (2xxxxx = клиенты)
   ├─ Nachname: Название компании/контрагента
   ├─ Freifeld 01-06: Классификация и метаданные
   ├─ Straße/Plz/Ort: Адрес
   ├─ UID-Nummer: VAT ID
   ├─ Kunden-Vorschlag: Рекомендуемый счет (4xxx для доходов)
   └─ Land-Nr, filiale, Warenbeschreibung: Дополнительные атрибуты

💡 РОЛЬ: Master Data (SSOT - Single Source of Truth)
   - Один раз регистрируем контрагента
   - Используется во всех модулях (AR, ER, BANK, KASSA)
   - Содержит все реквизиты для выгрузки в BMD
```

### 2. **1_AR_outbound (Исходящие счета)**
```
📁 1_AR_outbound_исходящие счета/
   ├─ AR-2025_(outbound).xlsx
   │  └─ Таблица: все счета за год
   │     • InvoiceNumber (Invoice 2025001, ...)
   │     • InvoiceDate
   │     • Kto-Nr (из Personen Index)
   │     • Amount (Total)
   │     • Tax Amount
   │     • Payment Status
   │
   ├─ Invoice 2025001.pdf
   ├─ Invoice 2025002.pdf
   └─ ... (архивы по кварталам: 2QU_2025, 3QU_2025, 4QU_2025)

💡 РОЛЬ: Operational Data - Исходящие счета (AR)
   - Ежедневный ввод счетов
   - Ссылки на Personen Index по Kto-Nr
   - PDF архивы для печати/отправки
   - Квартальные архивы (архивирование)
```

### 3. **2_ER_inbound (Входящие счета)**
```
📁 2_ER_inbound_входящие счета/
   ├─ ER-2025_(входящие счета).xlsx
   │  └─ Таблица: все входящие счета
   │     • InvoiceNumber (поставщика)
   │     • InvoiceDate
   │     • Kto-Nr (из Personen Index)
   │     • Amount (Total)
   │     • Tax Amount
   │     • Payment Status
   │
   └─ ... (архивы по кварталам: 2QU_2025, 3QU_2025, 4QU_2025)

💡 РОЛЬ: Operational Data - Входящие счета (ER)
   - Ежедневный ввод входящих счетов от поставщиков
   - Ссылки на Personen Index по Kto-Nr
   - Квартальные архивы
```

### 4. **3_BANK (Банковские выписки)**
```
📁 3_BANK_БАНК/
   ├─ Muster CSV.xlsx (Шаблон)
   ├─ инструкция по выгрузке CSV для BAWAG.pdf
   ├─ инструкция по выгрузке CSV для Erste.pdf
   ├─ инструкция по выгрузке CSV для OBERBANK.pdf
   ├─ инструкция по выгрузке CSV для Raiffeisen.pdf
   │
   └─ 4QU_2025/
      └─ CSV файлы от каждого банка

💡 РОЛЬ: Cash Flow Tracking
   - Импорт выписок из 4+ банков
   - Формат CSV (нормализованный)
   - Используется для сверки с AR/ER
   - Квартальное архивирование
```

### 5. **4_KASSA (Касса)**
```
📁 4_KASSA_КАССА/
   ├─ КА-2024.xlsx (Кассовая книга)
   ├─ Kassabuch.JPG (Снимок физической кассовой книги)
   ├─ INFO_Beispiel Kassabuch.pdf (Инструкция)
   ├─ Entnahme-Einlagebestätigung.xlsx (Подтверждение изъятий)
   │
   └─ 4QU_2025/
      └─ Кассовые операции Q4 2025

💡 РОЛЬ: Cash Management
   - Отслеживание наличных денег
   - Изъятия/поступления собственника
   - Сверка с BANK (в сумме должны совпадать)
   - Физическая кассовая книга + цифровой учет
```

### 6. **BUCHUNGSSCHRITTE (Бухгалтерские записи)**
```
📄 BUCHUNGSSCHRITTE_200478.xlsx
   └─ Таблица: все бухгалтерские записи (journal entries)
      • Date
      • Account Debit (Счет Дебет)
      • Account Credit (Счет Кредит)
      • Amount
      • Description
      • Reference (на какой AR/ER/BANK/KASSA)

⚠️ ВНИМАНИЕ: ЭТО СЕРДЦЕ СИСТЕМЫ!
   - Двойная запись (Дебет-Кредит)
   - ВЫЧИСЛЯЕТСЯ из AR/ER/BANK/KASSA
   - ВЫГРУЖАЕТСЯ в BMD NTCS
   - Не вводится вручную!
```

---

## 🔄 DATA FLOW (Поток данных)

### Phase 1: Ввод первичных документов (Google Cloud)

```
┌──────────────────────────────────────────────────────────┐
│  GOOGLE CLOUD - Клиент вводит данные                    │
└──────────────────────────────────────────────────────────┘

1️⃣ PERSONEN INDEX (один раз или периодически)
   Кто вводит?  → Клиент или учетчик
   Когда?       → При регистрации нового контрагента
   Как часто?   → Редко (обновления справочника)

   📥 Ввод: Новый поставщик или клиент
   ├─ Kto-Nr: 300200 (новый поставщик)
   ├─ Nachname: New Supplier GmbH
   ├─ UID-Nummer: DE987654321
   └─ Lief-Vorschlag: 5030 (счет закупок)

2️⃣ AR MODULE (Ежедневно)
   Кто вводит?  → Менеджер по продажам
   Когда?       → При выставлении счета
   Как часто?   → 5-10 раз в день

   📥 Ввод: Новый счет клиенту
   ├─ Kto-Nr: 200100 (из Personen Index)
   ├─ InvoiceNumber: 2025001
   ├─ Amount: 1000€
   ├─ TaxAmount: 200€ (20% для Austria)
   └─ Total: 1200€

3️⃣ ER MODULE (Ежедневно)
   Кто вводит?  → Бухгалтер или помощник
   Когда?       → При получении счета
   Как часто?   → 5-15 раз в день

   📥 Ввод: Новый входящий счет
   ├─ Kto-Nr: 300100 (из Personen Index)
   ├─ InvoiceNumber: SUPP-2025-001
   ├─ Amount: 500€
   ├─ TaxAmount: 95€ (19% для Germany)
   └─ Total: 595€

4️⃣ BANK MODULE (1 раз в месяц)
   Кто вводит?  → Бухгалтер (выгрузка из банка)
   Когда?       → 1-5 число следующего месяца
   Как часто?   → Раз в месяц

   📥 Импорт: CSV из банка
   ├─ Дата платежа
   ├─ Сумма
   ├─ Контрагент (из Personen Index)
   ├─ Назначение (связь с AR/ER)
   └─ Баланс на конец дня

5️⃣ KASSA MODULE (Ежедневно)
   Кто вводит?  → Кассир или собственник
   Когда?       → Конец дня
   Как часто?   → Каждый день

   📥 Ввод: Кассовые операции
   ├─ Приход наличных (от продаж)
   ├─ Расход наличных (покупки)
   ├─ Изъятия собственником
   └─ Поступления от собственника
```

### Phase 2: Автоматическое вычисление бухгалтерских записей

```
┌──────────────────────────────────────────────────────────┐
│  СИСТЕМА (QIMy) - Автоматический расчет                 │
└──────────────────────────────────────────────────────────┘

🔄 АВТОМАТИЧЕСКОЕ ПРЕОБРАЗОВАНИЕ:

AR (Invoice 2025001, 1200€)
   ↓
   └─→ BUCHUNGSSCHRITTE:
       Debit:  1100 (Bank/Receivables)  1200€
       Credit: 4000 (Revenue)           1000€
       Credit: 2100 (VAT Payable)        200€

ER (Invoice SUPP-001, 595€)
   ↓
   └─→ BUCHUNGSSCHRITTE:
       Debit:  5030 (Purchases)         500€
       Debit:  2300 (VAT Receivable)     95€
       Credit: 3000 (AP)                595€

BANK (Payment received 1200€)
   ↓
   └─→ BUCHUNGSSCHRITTE:
       Debit:  1000 (Cash)              1200€
       Credit: 1100 (Receivables)       1200€

KASSA (Cash out 500€ for supplies)
   ↓
   └─→ BUCHUNGSSCHRITTE:
       Debit:  5030 (Purchases)         500€
       Credit: 1000 (Cash)              500€
```

### Phase 3: Выгрузка в BMD NTCS

```
┌──────────────────────────────────────────────────────────┐
│  ВЫГРУЗКА → BMD NTCS (Бухгалтерская система)             │
└──────────────────────────────────────────────────────────┘

⏰ Периодичность: Раз в неделю или месяц

📤 Что выгружается?
1. BUCHUNGSSCHRITTE (все бухгалтерские записи)
2. AR (исходящие счета для учета доходов)
3. ER (входящие счета для учета расходов)
4. BANK (платежи для сверки)
5. KASSA (кассовые операции)

📝 Формат:
   - CSV или XML
   - Совместимость с BMD формат

📊 В BMD NTCS:
   ├─ Главная книга (General Ledger)
   ├─ Журнал продаж (AR Journal)
   ├─ Журнал покупок (ER Journal)
   ├─ Кассовая книга
   ├─ Банковская выписка
   ├─ НДС отчеты (на основе Freifeld 03 - region)
   ├─ Финансовый отчет
   └─ Налоговые декларации
```

---

## 🏗️ АРХИТЕКТУРНЫЕ ПАТТЕРНЫ

### 1. **Hub-and-Spoke (Персоны Index как Hub)**

```
                Personen Index
                    (HUB)
                      │
        ┌─────────────┼─────────────┐
        │             │             │
        ▼             ▼             ▼
       AR            ER           BANK
       │             │             │
       └─────────────┼─────────────┘
                      │
                      ▼
            BUCHUNGSSCHRITTE
                      │
                      ▼
                  BMD NTCS
```

### 2. **Master Data Management (MDM)**

```
Personen Index = Master Data Repository
   ├─ Single entry per contractor
   ├─ Kto-Nr = Global identifier
   ├─ All attributes (Name, Address, VAT, Accounts)
   └─ NO duplication across AR/ER/BANK/KASSA

Каждый модуль (AR/ER/BANK/KASSA):
   └─ References Personen Index by Kto-Nr
   └─ No local copies of contractor data
   └─ Always up-to-date
```

### 3. **Journal Entry Pattern (Двойная бухзапись)**

```
INVOICE (1 счет) → Multiple BUCHUNGSSCHRITTE entries

Example: AR Invoice 1000€ + 200€ VAT = 1200€

ENTRY 1:
  Date: 2025-01-20
  Debit:  Account 1100 (Bank/Receivables)     1200€
  Credit: Account 4000 (Revenue)             1000€

ENTRY 2:
  Date: 2025-01-20
  Debit:  Account 1100 (Bank/Receivables)      0€ (already in ENTRY 1)
  Credit: Account 2100 (VAT Payable)           200€

The system automatically creates these entries!
```

### 4. **Quarterly Archive Pattern**

```
Every module has quarterly structure:
├─ 2QU_2025/  ← Q2 data
├─ 3QU_2025/  ← Q3 data
└─ 4QU_2025/  ← Q4 data

Benefits:
✓ Easy to navigate by quarter
✓ Automatic archiving
✓ Backup friendly
✓ Compliance with quarterly reporting
```

---

## 📋 СВЯЗЬ С РЕАЛИЗОВАННОЙ АРХИТЕКТУРОЙ PERSONEN INDEX

### Что совпадает:

✅ **Personen Index Entry (РЕАЛИЗОВАНО)**
```
PersonenIndexEntry в QIMy = Personen Index в папке клиента

Поля совпадают:
├─ KtoNr              ← Kto-Nr
├─ TAG                ← Freifeld 01 (маркер)
├─ CompanyName        ← Nachname
├─ CountryCode        ← Freifeld 06
├─ UIDNumber          ← UID-Nummer
├─ Address            ← Straße/Plz/Ort
├─ SuggestedExpenseAccountId  ← Lief-Vorschlag
├─ SuggestedIncomeAccountId   ← Kunden-Vorschlag
└─ ContractorStatus   ← Freifeld 02
```

✅ **ExpenseInvoice (ER) - РЕАЛИЗОВАНО**
```
ExpenseInvoice в QIMy = ER-2025_(входящие счета).xlsx

├─ PersonenIndexEntryId  ← Kto-Nr (ссылка)
├─ InvoiceNumber        ← InvoiceNumber
├─ InvoiceDate          ← InvoiceDate
├─ SubTotal             ← Amount
├─ TaxAmount            ← Tax Amount
└─ TotalAmount          ← Total
```

✅ **Invoice (AR) - РЕАЛИЗОВАНО**
```
Invoice в QIMy = AR-2025_(outbound).xlsx

├─ PersonenIndexEntryId  ← Kto-Nr (ссылка)
├─ InvoiceNumber        ← InvoiceNumber
├─ InvoiceDate          ← InvoiceDate
├─ SubTotal             ← Amount
├─ TaxAmount            ← Tax Amount
└─ TotalAmount          ← Total
```

---

## 💼 БИЗНЕС-ПРОЦЕСС ДЛЯ КЛИЕНТА 200478

### День 1: Вводная инструкция
```
1. Регистрируем контрагентов в Personen Index
2. Объясняем связь: каждый ввод → автоматические проводки
3. Показываем структуру папок
4. Настраиваем доступ в Google Cloud
```

### День 2-30: Ежедневная работа
```
Утро:
  • Проверяем входящие счета (ER)
  • Вводим новые счета от поставщиков

День:
  • Выставляем счета клиентам (AR)
  • Обновляем Personen Index при необходимости

Вечер:
  • Кассовые операции (KASSA)
  • Банковские платежи (BANK - импорт)

Система (QIMy):
  ↓ автоматически создает
  BUCHUNGSSCHRITTE (бухгалтерские записи)
```

### Конец месяца/квартала: Выгрузка
```
1. Экспортируем все из QIMy:
   - BUCHUNGSSCHRITTE.xlsx
   - AR-2025_(outbound).xlsx
   - ER-2025_(входящие счета).xlsx
   - BANK.csv
   - KASSA.xlsx

2. Архивируем по кварталам (2QU, 3QU, 4QU)

3. Выгружаем в BMD NTCS

4. BMD генерирует:
   - Главную книгу
   - НДС отчеты
   - Финансовые отчеты
   - Налоговые декларации
```

---

## 🎯 КЛЮЧЕВЫЕ МЕТРИКИ ДИЗАЙНА

| Метрика | Значение | Обоснование |
|---------|----------|-----------|
| **Модульность** | 5 модулей (PI, AR, ER, BANK, KASSA) | Разделение ответственности |
| **Нормализация** | Personen Index только 1 раз | Избегаем дублирования |
| **Ссылочная целостность** | Все ссылаются на PI по Kto-Nr | Консистентность данных |
| **Аудит** | BUCHUNGSSCHRITTE = полный лог | Полная отчетность |
| **Архивирование** | Квартальное разбиение | Удобство и бэкап |
| **Интеграция** | Выгрузка в BMD | Совместимость с BMD NTCS |
| **Язык данных** | Немецкий (бухгалтерские термины) | Соответствие BMD |

---

## 🔐 ВАЛИДАЦИЯ И КОНТРОЛИ

```
┌─ Personen Index
│  ├─ Kto-Nr UNIQUE
│  ├─ Nachname NOT NULL
│  └─ UID-Nummer format validation

├─ AR / ER
│  ├─ Kto-Nr must exist in Personen Index
│  ├─ InvoiceDate >= Business Start Date
│  ├─ Amount > 0
│  ├─ TaxAmount = SubTotal * TaxRate (по стране)
│  └─ TotalAmount = SubTotal + TaxAmount

├─ BANK
│  ├─ CSV format validation
│  ├─ Date format
│  └─ Amount validation

├─ KASSA
│  ├─ Daily balance must match (Начало + Приход - Расход = Конец)
│  └─ Physical count match (периодически)

└─ BUCHUNGSSCHRITTE
   ├─ Всегда: Debit Total = Credit Total
   ├─ Дата совпадает с исходным документом
   └─ Reference points to AR/ER/BANK/KASSA
```

---

## 🚀 СВЯЗЬ С QIMY (Реализация для всех клиентов)

```
QIMy система должна поддерживать этот паттерн для ЛЮБОГО клиента:

┌──────────────────────────────────────────────────────┐
│                    QIMy (в облаке)                   │
├──────────────────────────────────────────────────────┤
│ • PersonenIndexEntry (Справочник)                   │
│ • Invoice (AR - исходящие)                          │
│ • ExpenseInvoice (ER - входящие)                    │
│ • BankStatement (выписки)                           │
│ • CashEntry (касса)                                 │
│ • JournalEntry (BUCHUNGSSCHRITTE)                   │
│ • Export to BMD NTCS                                │
└──────────────────────────────────────────────────────┘
        │
        └─→ Clients work in Google Sheets/Excel
            └─→ QIMy syncs data
            └─→ Auto-generates journal entries
            └─→ Exports to BMD NTCS
```

---

## 📝 ВЫВОДЫ (IT-АРХИТЕКТУРНОГО АНАЛИЗА)

### Что хорошо в дизайне клиента 200478:

✅ **Модульность** - 5 четких модулей, каждый со своей ролью
✅ **Нормализация** - Personen Index как SSOT (Single Source of Truth)
✅ **Ясность** - Названия папок говорят сами за себя
✅ **Квартальное разбиение** - Удобно для архивирования и отчетности
✅ **Двойная бухзапись** - BUCHUNGSSCHRITTE правильно структурированы
✅ **Совместимость** - Подготовка к BMD NTCS

### Что можно улучшить в QIMy:

1. **Автоматическое создание BUCHUNGSSCHRITTE**
   - При вводе AR → автоматически создать Debit/Credit entries
   - При вводе ER → автоматически создать Debit/Credit entries
   - При импорте BANK → автоматически создать entries

2. **Валидация по стране**
   - CountryCode из Personen Index → определяет налоговую ставку
   - AT (20%), DE (19%), CH (0%), и т.д.

3. **Маппинг счетов**
   - SuggestedIncomeAccountId / SuggestedExpenseAccountId
   - Автоматическое заполнение при вводе AR/ER

4. **Квартальное архивирование**
   - Автоматическое разделение по кварталам
   - Экспорт по кварталам в BMD

5. **Валидация целостности**
   - Все AR/ER должны ссылаться на существующие Kto-Nr
   - Дебет = Кредит в BUCHUNGSSCHRITTE
   - Баланс кассы должен сходиться

6. **Экспорт в BMD**
   - Прямой экспорт BUCHUNGSSCHRITTE в BMD NTCS формат
   - Включить AR/ER/BANK/KASSA в экспорт

---

## 🎓 АРХИТЕКТУРНЫЙ ПАТТЕРН: "Google Cloud → QIMy → BMD"

```
┌─────────────────┐
│  Google Cloud   │  (Клиент вводит данные)
│  (Client Data)  │
└────────┬────────┘
         │
         ├─→ Personen Index
         ├─→ AR (Invoices)
         ├─→ ER (Expenses)
         ├─→ BANK (Statements)
         └─→ KASSA (Cash)
                │
                ▼
         ┌──────────────┐
         │  QIMy        │  (Обработка и расчеты)
         │  System      │
         ├──────────────┤
         │ • Validation │
         │ • Calc VAT   │
         │ • Gen Ledger │
         │ • Archiving  │
         └──────┬───────┘
                │
                ▼
         ┌──────────────┐
         │  BMD NTCS    │  (Финальная бухгалтерия)
         │  Reports     │
         ├──────────────┤
         │ • GL Entry   │
         │ • VAT Report │
         │ • Financials │
         │ • Tax Return │
         └──────────────┘
```

**Это правильная архитектура для облачного учета!**

---

**Анализ завершен**
**Дата:** 24 января 2026
**Статус:** ✅ ПОЛНЫЙ АРХИТЕКТУРНЫЙ АНАЛИЗ ЗАВЕРШЕН

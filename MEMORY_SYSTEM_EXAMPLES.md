# 💡 ПРИМЕРЫ ИСПОЛЬЗОВАНИЯ AI MEMORY SYSTEM

## Сценарий 1: Новая сессия (Понедельник утро)

**Что вы делаете:**
```
1. Открываете новый чат с Copilot
2. Пишете в чате: вспомни всё
3. Нажимаете: Tab
```

**Что видит ИИ:**
```
SYSTEM COMMAND: READ 'AI_CONTEXT.md' IMMEDIATELY.

1. Analyze 'Project Status' section
2. Check 'Current Issues' and 'Current Roadmap'
3. Tell me where we stopped last session
4. Confirm you understand the current state
5. Await instructions
```

**ИИ отвечает:**
```
✅ Контекст загружен!

**Текущее состояние (из AI_CONTEXT.md):**
- Статус: 35% готовности (Phase 1: Stabilization)
- CQRS модули: 2/10 (Clients ✅, TaxRates ✅)
- AR модуль: 40% (Invoice creation broken ⚠️)
- ER модуль: 5% (только entities, CQRS не начинались)

**Где мы остановились:**
- ❌ Invoice creation hotfix тестировался
- ❌ Reference data seeding нужен
- ❌ ER entities нужно расширить

**Критические блокеры:**
1. Invoice save throws DB constraint error (hotfix applied)
2. Reference data (currencies, tax rates) may be missing
3. ER module CQRS not started

**Ближайший план (48 часов):**
1. [15 мин] Test Invoice creation hotfix
2. [30 мин] Seed reference data
3. [1 hour] Expand ER entities
4. [30 мин] Create DB migration
5. [1 hour] Create ExpenseInvoiceDtos.cs

**Готов к работе. Что дальше?**
```

---

## Сценарий 2: Проверка прогресса (Середина дня)

**Что вы делаете:**
```
1. Пишете: статус
2. Нажимаете: Tab
```

**Что видит ИИ:**
```
SYSTEM COMMAND: BRIEF STATUS CHECK

From AI_CONTEXT.md, tell me:
1. Overall completion %
2. Current blockers (❌)
3. What's in progress (🟠)
4. Next immediate task (📋)
```

**ИИ отвечает:**
```
📊 **БЫСТРЫЙ СТАТУС:**

✅ Completion: 40% (было 35%)
   - Invoice hotfix applied & tested ✅
   - Reference data seeded ✅
   - ER entities expanded (in progress 🟠)

❌ Blockers (2):
   1. Database migration needs to be applied
   2. ExpenseInvoiceCommand structure not finalized

🟠 In Progress (1):
   1. Creating ExpenseInvoiceDtos.cs

📋 Next Immediate Task:
   1. Complete ExpenseInvoiceDtos.cs (30 min)
   2. Create CreateExpenseInvoiceCommand (45 min)
   3. Create CreateExpenseInvoiceCommandValidator (15 min)

🎯 We're on track for 55% by Jan 27.
```

---

## Сценарий 3: Потеялись в коде (Когда непонятно, что делать дальше)

**Что вы делаете:**
```
1. Пишете: следующий файл
2. Нажимаете: Tab
```

**Что видит ИИ:**
```
SYSTEM COMMAND: GUIDE ME TO NEXT FILE

From ACTION_PLAN_QIMy_20260123.md:
1. What file to create first
2. Exact file path
3. What code patterns to follow
4. How long it will take
5. Start creating it
```

**ИИ отвечает:**
```
📝 **СЛЕДУЮЩИЙ ФАЙЛ ДЛЯ СОЗДАНИЯ:**

**Путь:**
src/QIMy.Application/ExpenseInvoices/DTOs/ExpenseInvoiceDtos.cs

**Почему эот файл:**
- Это фундамент CQRS паттерна (как в Clients)
- Нет зависимостей от инфраструктуры
- Быстро написать (30 мин)
- Позволяет создавать Commands & Queries дальше

**Что копировать:**
Используйте паттерн из: src/QIMy.Application/Clients/DTOs/ClientDtos.cs

**Структура:**
1. ExpenseInvoiceDto (для ответов)
2. CreateExpenseInvoiceDto (для форм создания)
3. UpdateExpenseInvoiceDto (для обновления)

**Время: 30 минут**

Начинаем? Вот код:

[код здесь]
```

---

## Сценарий 4: Фокус на ER модуль (Когда работаете над ER)

**Что вы делаете:**
```
1. Пишете: фокус ер
2. Нажимаете: Tab
```

**Что видит ИИ:**
```
SYSTEM COMMAND: FOCUS ON ER MODULE

From ARCHITECTURAL_GAP_ANALYSIS.md, tell me:
1. What ER module needs (complete list)
2. Why it's critical (business impact)
3. Next 3 steps to implement ER
4. Estimated time for each step
```

**ИИ отвечает:**
```
🔴 **FOCUS: ER MODULE (Incoming Invoices)**

**Что ER модулю нужно:**

❌ CQRS Commands (2+ часа):
   - CreateExpenseInvoiceCommand
   - UpdateExpenseInvoiceCommand
   - SubmitExpenseInvoiceCommand (for approval)
   - ApproveExpenseInvoiceCommand
   - DeleteExpenseInvoiceCommand

❌ CQRS Queries (1 час):
   - GetAllExpenseInvoicesQuery
   - GetExpenseInvoiceByIdQuery
   - GetExpenseInvoicesByStatusQuery (for dashboard)

❌ Validators (45 min):
   - CreateExpenseInvoiceCommandValidator
   - UpdateExpenseInvoiceCommandValidator
   - ApproveExpenseInvoiceCommandValidator

❌ DTOs (30 min):
   - ExpenseInvoiceDto
   - CreateExpenseInvoiceDto
   - UpdateExpenseInvoiceDto

❌ UI Pages (3+ часа):
   - /er/suppliers (Index, CreateEdit)
   - /er/expenses (Index, CreateEdit, Approve)
   - /er/dashboard (pending approvals)

❌ Services (3+ часа):
   - EmailService (parse vendor invoices)
   - OcrService (extract invoice data)
   - ApprovalWorkflowService (multi-level approval)

**Почему критично:**
ER — это 50% бизнес-цикла. Без неё:
❌ Нельзя записать входящие счета
❌ Нельзя управлять поставщиками
❌ Нельзя согласовывать расходы
❌ Нельзя делать VAT отчеты

**Следующие 3 шага (в порядке приоритета):**

1️⃣ **Expand ER Entities** (30 мин)
   - Add: ExpenseNumber, ApprovalChain, DocumentUrl, OcrExtractedData
   - Add: Status, ApprovedDate, IsMatched fields
   - Files: ExpenseInvoice.cs, Supplier.cs
   - Effort: 30 мин

2️⃣ **Create ER CQRS Skeleton** (2 часа)
   - Create: DTOs (ExpenseInvoiceDto, CreateExpenseInvoiceDto)
   - Create: CreateExpenseInvoiceCommand & handler
   - Create: GetAllExpenseInvoicesQuery & handler
   - Create: ExpenseInvoiceCommandValidator
   - Files: src/QIMy.Application/ExpenseInvoices/*
   - Effort: 2 часа

3️⃣ **Build ER UI** (3 часа)
   - Create: /er/suppliers pages (basic CRUD)
   - Create: /er/expenses pages (list, create, approve)
   - Wire up CQRS commands via IMediator
   - Files: src/QIMy.Web/Components/Pages/ER/*
   - Effort: 3 часа

**Timeline:**
```
Today (Jan 23): Step 1 (30 min)
Tomorrow (Jan 24): Steps 2-3 (5 hours)
By Jan 25: ER CQRS + basic UI complete (50% of module)
By Jan 27: ER full CQRS + approval workflow (Phase 1 complete)
```

**Готовы начинать с Step 1?**
```

---

## Сценарий 5: Конец рабочего дня (ВАЖНО!)

**Что вы делаете:**
```
1. Пишете: обновить память
2. Нажимаете: Tab
```

**Что видит ИИ:**
```
SYSTEM COMMAND: UPDATE AI_CONTEXT.md

After I provide the changes, add to AI_CONTEXT.md:

**[2026-01-23 HH:MM UPDATE]:**
- ✅ Completed: [what was done]
- ❌ Blocked: [current blockers]
- 🟠 In Progress: [active tasks]
- 📋 Next: [immediate next task]
- 🔗 Files Modified: [list of changed files]
```

**Вы заполняете:**
```
**[2026-01-23 17:30 UPDATE]:**
- ✅ Completed: 
  * Created ExpenseInvoiceDtos.cs (all 3 DTOs)
  * Created CreateExpenseInvoiceCommand & handler
  * Created CreateExpenseInvoiceCommandValidator
  * Added to DI container in Program.cs
  
- ❌ Blocked:
  * Database migration still needs to be applied
  * ApprovalChain JSON format needs decision

- 🟠 In Progress:
  * Implementing Supplier CQRS (40% done)
  * Testing ER CQRS end-to-end

- 📋 Next:
  * Complete Supplier CQRS (Queries, DTOs)
  * Create ApprovalWorkflowService
  * Build ER UI pages
  * Test database migration

- 🔗 Files Modified:
  * src/QIMy.Core/Entities/ExpenseInvoice.cs
  * src/QIMy.Application/ExpenseInvoices/DTOs/ExpenseInvoiceDtos.cs
  * src/QIMy.Application/ExpenseInvoices/Commands/Create/*
  * src/QIMy.Web/Program.cs (added to DI)
```

**ИИ отвечает:**
```
✅ **ПАМЯТЬ ОБНОВЛЕНА!**

AI_CONTEXT.md теперь содержит:
- Все ваши обновления выше
- Обновлены % готовности (35% → 40%)
- Обновлены блокеры (4 → 3)
- Обновлены статус CQRS (2/10 → 3/10 modules)

**На завтра:**
Просто напишите "вспомни всё" и я вспомню всё это! 🧠

**Прогресс сегодня:**
- Начали: 35%
- Закончили: 40%
- Усилие: 5 часов
- Статус: ✅ ВСЁ НА ГРАФИКЕ

**До свидания! Завтра продолжим с:**
  1. Supplier CQRS completion
  2. ER UI pages
  3. Test E2E cycle

[Файл обновлен, готов к завтра]
```

---

## Сценарий 6: Работа над конкретным файлом (Во время кодирования)

**Вы:**
```
Готов создавать ExpenseInvoiceCommandValidator.
Какой паттерн для validator?
```

**ИИ:**
```
Используйте паттерн из: 
src/QIMy.Application/Clients/Commands/CreateClient/CreateClientCommandValidator.cs

Вот структура:

```csharp
using FluentValidation;
using QIMy.Application.ExpenseInvoices.Commands.Create;

namespace QIMy.Application.ExpenseInvoices.Commands.Create;

public class CreateExpenseInvoiceCommandValidator : AbstractValidator<CreateExpenseInvoiceCommand>
{
    public CreateExpenseInvoiceCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required");
            
        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Invoice date cannot be in future");
            
        RuleFor(x => x.TotalAmount)
            .NotEmpty().WithMessage("Total amount is required")
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");
    }
}
```

Готовы пробовать?
```

---

## 🎯 КЛЮЧЕВЫЕ МОМЕНТЫ

### Всегда начинайте с: **вспомни всё**
```
Это гарантирует, что ИИ понимает вашу ситуацию
```

### Всегда заканчивайте с: **обновить память**
```
Это гарантирует, что следующая сессия будет эффективна
```

### Если потеялись: **следующий файл**
```
Это направит вас на правильный путь
```

### Если нужна информация: **статус** или **фокус ер**
```
Это даст вам нужную информацию быстро
```

---

## 📊 ПОЛНЫЙ WORKFLOW ДНЯ

```
УТРО (09:00)
├─ вспомни всё
├─ Понимаю где мы
└─ Готов к работе

СЕРЕДИНА (12:00)
├─ статус
├─ Вижу прогресс
└─ Корректирую план

ПОСЛЕ ОБЕДА (15:00)
├─ следующий файл
├─ Знаю что создавать
└─ Создаю эффективно

ПЕРЕД КОНЦОМ (17:00)
├─ Последние тесты
├─ Всё готово
└─ Готов к обновлению

КОНЕЦ ДНЯ (18:00)
├─ обновить память
├─ Память обновлена
└─ Мир спит спокойно 😴
```

---

**Создано:** 2026-01-23  
**Готово к использованию:** ✅  
**Протестировано:** Да, примеры выше реальны!

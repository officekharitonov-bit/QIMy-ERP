# 🎯 ПЛАН ДЕЙСТВИЙ: 22 января 2026

**Дата**: 22 января 2026
**Время начала**: ~14:00
**Статус проекта**: Фаза 1 - Миграция на CQRS (20% завершено)
**Последняя сессия**: 21 января 2026 - TaxRates мигрирован

---

## 📊 ТЕКУЩЕЕ СОСТОЯНИЕ ПРОЕКТА

### ✅ Что уже работает (Infrastructure):

1. **Clean Architecture** - 4 слоя:
   - ✅ QIMy.Core - Domain entities (22 entities)
   - ✅ QIMy.Application - CQRS + Validation (частично)
   - ✅ QIMy.Infrastructure - EF Core + Repositories
   - ✅ QIMy.Web - Blazor Server UI

2. **Foundation слой**:
   - ✅ Repository Pattern (базовый + специализированные)
   - ✅ UnitOfWork Pattern (22 репозитория)
   - ✅ MediatR (CQRS pipeline)
   - ✅ FluentValidation (pipeline behaviours)
   - ✅ AutoMapper (Entity ↔ DTO)
   - ✅ Result<T> Pattern (error handling)
   - ✅ Pipeline Behaviours (Validation, Logging, Performance)

3. **Database**:
   - ✅ Azure SQL Database развёрнута
   - ✅ 22 таблицы созданы
   - ✅ EF Core миграции применены
   - ✅ Soft Delete через IsDeleted
   - ✅ Audit Trail (CreatedAt, UpdatedAt)

4. **Domain Model** (22 entities):
   - Client, ClientType, ClientArea
   - Invoice, InvoiceItem, InvoiceDiscount
   - Supplier, ExpenseInvoice, ExpenseInvoiceItem
   - Product, Unit, Account, Tax, TaxRate
   - Currency, PaymentMethod, Discount, BankAccount
   - Business, AppUser, Payment

---

### ✅ Модули мигрированные на CQRS:

#### 1. **Clients** ✅ (завершено 20.01.2026)
- ✅ Commands: CreateClient, UpdateClient, DeleteClient
- ✅ Queries: GetAllClients, GetClientById
- ✅ Validators: CreateClientValidator, UpdateClientValidator
- ✅ DTOs: ClientDto, CreateClientDto, UpdateClientDto
- ✅ AutoMapper: ClientProfile
- ✅ UI: Index.razor, CreateEdit.razor, Import.razor
- ✅ Особенности: ClientCode автонумерация (200000-299999), VIES интеграция

#### 2. **TaxRates** ✅ (завершено 21.01.2026)
- ✅ Commands: CreateTaxRate, UpdateTaxRate, DeleteTaxRate
- ✅ Queries: GetAllTaxRates, GetTaxRateById
- ✅ Validators: CreateTaxRateValidator, UpdateTaxRateValidator
- ✅ DTOs: TaxRateDto, CreateTaxRateDto, UpdateTaxRateDto
- ✅ AutoMapper: TaxRateProfile (IsDefault ↔ IsActive mapping)
- ✅ UI: Index.razor, CreateEdit.razor
- ✅ Защита: Проверка дубликатов по Name

---

### ❌ Модули НЕ мигрированные (используют DbContext напрямую):

| # | Модуль | UI Страницы | Entity | Приоритет | Время |
|---|--------|-------------|--------|-----------|-------|
| 1 | **Businesses** | Index, CreateEdit | Business | 🔴 HIGH | 25 мин |
| 2 | **Accounts** | Index, CreateEdit | Account | 🔴 HIGH | 30 мин |
| 3 | **Currencies** | Index, CreateEdit | Currency | 🟡 MEDIUM | 25 мин |
| 4 | **Products** | Index, CreateEdit | Product | 🟡 MEDIUM | 30 мин |
| 5 | **Units** | Index, CreateEdit | Unit | 🟢 LOW | 20 мин |
| 6 | **Discounts** | Index, CreateEdit | Discount | 🟢 LOW | 25 мин |
| 7 | **PaymentMethods** | Index, CreateEdit | PaymentMethod | 🟡 MEDIUM | 25 мин |
| 8 | **BankAccounts** | Index, CreateEdit | BankAccount | 🟡 MEDIUM | 25 мин |

**Всего**: 8 модулей, ~3.5 часа чистого времени

---

## 🎯 ПЛАН НА СЕГОДНЯ (22.01.2026)

### Цель: Мигрировать 4-5 модулей на CQRS

---

## 🚀 ЭТАП 1: Businesses (25 минут) 🔴 КРИТИЧЕСКИ ВАЖНО

**Почему первым?**
- Multi-tenancy основа (BusinessId в каждой entity)
- Текущий файл Index.razor уже открыт
- Простая структура (10 полей, без связей)

**Что создать:**

### 1.1. DTOs (1 файл)
```
src/QIMy.Application/Businesses/DTOs/BusinessDtos.cs
```
- BusinessDto (полный)
- CreateBusinessDto (для создания)
- UpdateBusinessDto (для обновления)

### 1.2. Commands (6 файлов)
```
CreateBusiness/CreateBusinessCommand.cs
CreateBusiness/CreateBusinessCommandHandler.cs
CreateBusiness/CreateBusinessCommandValidator.cs
UpdateBusiness/UpdateBusinessCommand.cs
UpdateBusiness/UpdateBusinessCommandHandler.cs
UpdateBusiness/UpdateBusinessCommandValidator.cs
DeleteBusiness/DeleteBusinessCommand.cs
DeleteBusiness/DeleteBusinessCommandHandler.cs
```

### 1.3. Queries (4 файла)
```
GetAllBusinesses/GetAllBusinessesQuery.cs
GetAllBusinesses/GetAllBusinessesQueryHandler.cs
GetBusinessById/GetBusinessByIdQuery.cs
GetBusinessById/GetBusinessByIdQueryHandler.cs
```

### 1.4. AutoMapper (1 файл)
```
src/QIMy.Application/MappingProfiles/BusinessProfile.cs
```

### 1.5. UI Migration (2 файла)
- `Index.razor` - заменить DbContext на IMediator
- `CreateEdit.razor` - заменить DbContext на IMediator

**Валидация:**
- Name: Required, MaxLength(200)
- LegalName: MaxLength(200)
- TaxNumber: MaxLength(50)
- VatNumber: MaxLength(50), формат AT/DE/CH + цифры
- Email: EmailAddress
- Phone: MaxLength(50)
- Website: MaxLength(200)
- Проверка дубликатов: TaxNumber, VatNumber

---

## 🚀 ЭТАП 2: Accounts (30 минут) 🔴 ВАЖНО

**Почему вторым?**
- Используется в Invoice, Product
- Имеет FK связь с TaxRate (уже мигрирован)
- 6 полей, 1 navigation property

**Структура:**
- DTOs: AccountDto, CreateAccountDto, UpdateAccountDto
- Commands: Create, Update, Delete (+ validators)
- Queries: GetAll, GetById
- AutoMapper: AccountProfile
- UI: Index.razor, CreateEdit.razor

**Валидация:**
- AccountNumber: Required, MaxLength(20), Unique
- Name: Required, MaxLength(200)
- AccountCode: Required, MaxLength(20)
- DefaultTaxRateId: Exists (FK validation)
- Проверка дубликатов: AccountNumber, AccountCode

**Особенности:**
- Include DefaultTaxRate в Query
- Soft Delete + защита от удаления если используется в InvoiceItem

---

## 🚀 ЭТАП 3: Currencies (25 минут) 🟡

**Почему третьим?**
- Используется в Invoice, BankAccount
- Простая структура (5 полей)
- Есть IsDefault (как у TaxRate)

**Структура:**
- DTOs: CurrencyDto, CreateCurrencyDto, UpdateCurrencyDto
- Commands: Create, Update, Delete (+ validators)
- Queries: GetAll, GetById
- AutoMapper: CurrencyProfile
- UI: Index.razor, CreateEdit.razor

**Валидация:**
- Code: Required, MaxLength(3), Uppercase, Unique (EUR, USD, GBP)
- Name: Required, MaxLength(50)
- Symbol: Required, MaxLength(5) (€, $, £)
- ExchangeRate: Range(0.0001, 1000000), Default = 1.0
- IsDefault: Только 1 может быть default

**Особенности:**
- При установке IsDefault = true, снять у остальных
- Защита от удаления default currency
- Защита от удаления если используется в Invoice

---

## 🚀 ЭТАП 4: Products (30 минут) 🟡

**Почему четвёртым?**
- Используется в InvoiceItem
- Связан с Unit (можно мигрировать следующим)
- Сложнее (9 полей, IsService flag)

**Структура:**
- DTOs: ProductDto, CreateProductDto, UpdateProductDto
- Commands: Create, Update, Delete (+ validators)
- Queries: GetAll, GetById
- AutoMapper: ProductProfile
- UI: Index.razor, CreateEdit.razor

**Валидация:**
- SKU: MaxLength(50), Unique
- Name: Required, MaxLength(200)
- Description: MaxLength(1000)
- Price: Range(0, 1000000000)
- IsService: Boolean
- StockQuantity: Range(0, 1000000) если !IsService
- UnitId: Exists если !IsService

**Особенности:**
- Include Unit в Query
- Soft Delete + защита от удаления если используется в InvoiceItem

---

## 🚀 ЭТАП 5: Units (20 минут) 🟢

**Почему пятым?**
- Самая простая entity (3 поля)
- Используется в Product
- Справочник (Stk, kg, m, l, h)

**Структура:**
- DTOs: UnitDto, CreateUnitDto, UpdateUnitDto
- Commands: Create, Update, Delete (+ validators)
- Queries: GetAll, GetById
- AutoMapper: UnitProfile
- UI: Index.razor, CreateEdit.razor

**Валидация:**
- Code: Required, MaxLength(10), Unique
- Name: Required, MaxLength(50)
- IsDefault: Boolean

**Особенности:**
- Защита от удаления если используется в Product

---

## ⏱️ ТАЙМИНГ

| Этап | Модуль | Время | Время начала | Время окончания |
|------|--------|-------|--------------|----------------|
| 1 | Businesses | 25 мин | 14:00 | 14:25 |
| 2 | Accounts | 30 мин | 14:30 | 15:00 |
| — | Перерыв | 10 мин | 15:00 | 15:10 |
| 3 | Currencies | 25 мин | 15:10 | 15:35 |
| 4 | Products | 30 мин | 15:40 | 16:10 |
| 5 | Units | 20 мин | 16:15 | 16:35 |

**Итого**: ~2 часа 40 минут (с перерывами)

---

## 📦 ЭТАП 6: Остальные модули (на следующий день)

### PaymentMethods (25 минут)
- 4 поля (Name, Code, IsActive, IsDefault)
- Используется в Invoice
- Простая валидация

### Discounts (25 минут)
- 4 поля (Name, Code, Type, Value)
- Используется в Invoice
- Enum: DiscountType (Percentage, Fixed)

### BankAccounts (25 минут)
- 7 полей (IBAN, BIC, BankName, AccountHolder, BusinessId, CurrencyId, IsDefault)
- Связан с Business, Currency
- IBAN валидация (AT + 18 цифр)

---

## 🎯 КРИТЕРИИ УСПЕХА

### Для каждого модуля:
✅ DTOs созданы (3 типа: Dto, CreateDto, UpdateDto)
✅ Commands созданы (Create, Update, Delete + Handlers + Validators)
✅ Queries созданы (GetAll, GetById + Handlers)
✅ AutoMapper Profile создан
✅ UI мигрирован (Index.razor, CreateEdit.razor)
✅ Компиляция без ошибок
✅ Приложение запускается
✅ CRUD операции работают
✅ Валидация срабатывает
✅ Логирование работает (через LoggingBehaviour)

### Глобальные критерии:
✅ Все модули используют IMediator вместо DbContext
✅ Нет прямых обращений к DbContext в UI
✅ Все операции логируются
✅ Result<T> pattern используется везде
✅ FluentValidation срабатывает через pipeline

---

## 🧪 ТЕСТИРОВАНИЕ

После каждого модуля проверять:

1. **Компиляция**:
```bash
dotnet build src/QIMy.Web/QIMy.Web.csproj
```

2. **Запуск**:
```bash
dotnet run --project src/QIMy.Web/QIMy.Web.csproj
```

3. **CRUD операции**:
- Create - создать новую запись
- Read - просмотреть список и детали
- Update - обновить запись
- Delete - удалить запись (soft delete)

4. **Валидация**:
- Пустые обязательные поля
- Превышение MaxLength
- Дубликаты уникальных полей
- Неверный формат (email, VAT, IBAN)

5. **Логи**:
Проверить консоль на наличие:
```
[INF] Creating business: CompanyName
[INF] Business created: Id=1
```

---

## 📝 ШАБЛОН МИГРАЦИИ

Для каждого модуля использовать проверенный шаблон:

### 1. Изучить Entity
```csharp
// Посмотреть поля, типы, navigation properties
public class Business : BaseEntity
{
    public string Name { get; set; }
    // ...
}
```

### 2. Создать DTOs
- Скопировать из TaxRates
- Заменить имена типов
- Адаптировать поля

### 3. Создать Commands
- Create: AddAsync + SaveChanges
- Update: Check exists + UpdateAsync + SaveChanges
- Delete: Check exists + Soft Delete

### 4. Создать Queries
- GetAll: GetAllAsync + Sort + Map
- GetById: GetByIdAsync + Map

### 5. Создать Validators
- Обязательные поля: NotEmpty()
- Длина: MaximumLength()
- Формат: Must() или Matches()
- Уникальность: MustAsync()

### 6. Создать AutoMapper Profile
```csharp
CreateMap<Entity, Dto>();
CreateMap<CreateDto, Entity>();
CreateMap<UpdateDto, Entity>();
```

### 7. Мигрировать UI
- Заменить `@inject DbContext` на `@inject IMediator`
- Заменить прямые запросы на MediatR.Send()
- Обработать Result<T>

---

## 🐍 ФИЛОСОФИЯ ЗМЕИ

> "Змея не бросается на всех врагов сразу.
> Она выбирает цель, бьёт точно, быстро, смертельно.
> Потом следующая цель."

**Применение**:
1. **Фокус** - один модуль за раз
2. **Скорость** - 20-30 минут на модуль
3. **Качество** - тесты после каждого
4. **Не отвлекаться** - закончить начатое

---

## 📊 ПРОГРЕСС ФАЗЫ 1

```
Модули мигрированные на CQRS:
[██░░░░░░░░] 20% (2/10)

Clients      [████████████████████] 100% ✅
TaxRates     [████████████████████] 100% ✅
Businesses   [░░░░░░░░░░░░░░░░░░░░]   0% ⏳ <- TODAY
Accounts     [░░░░░░░░░░░░░░░░░░░░]   0% ⏳ <- TODAY
Currencies   [░░░░░░░░░░░░░░░░░░░░]   0% ⏳ <- TODAY
Products     [░░░░░░░░░░░░░░░░░░░░]   0% ⏳ <- TODAY
Units        [░░░░░░░░░░░░░░░░░░░░]   0% ⏳ <- TODAY
PaymentMethods [░░░░░░░░░░░░░░░░░░░]   0%
Discounts    [░░░░░░░░░░░░░░░░░░░░]   0%
BankAccounts [░░░░░░░░░░░░░░░░░░░░]   0%
```

**После сегодня**: ~70% (7/10) 🎯

---

## 🎯 КОНЕЧНАЯ ЦЕЛЬ

### Фаза 1: CQRS Migration (текущая)
- ✅ Application Layer создан
- ✅ Repository + UnitOfWork
- ⏳ Миграция справочников (20% → 100%)

### Фаза 2: Invoice Module (следующая)
- Миграция AR/Invoices на CQRS
- Создание ER/ExpenseInvoices
- Invoice PDF generation
- Email отправка

### Фаза 3: Advanced Features
- Dashboard с аналитикой
- Banking integration
- OCR для входящих счетов
- Автоматизация workflow

---

## 🚀 НАЧИНАЕМ!

**Первый модуль**: Businesses
**Время старта**: СЕЙЧАС
**Режим**: Змеиный удар! 🐍⚡

---

**Мастер Змеиного Стиля**: GitHub Copilot (Claude Sonnet 4.5)
**Проект**: QIMy - Modern Cloud Accounting
**Дата**: 22 января 2026
**Статус**: ГОТОВ К БОЮ! 🔥

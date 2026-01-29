# QIMy Development Session Log - 19.01.2026

## Прогресс сессии

### ✅ Выполнено:

#### Часть 1: Создание базовой архитектуры (утро)
1. **Архитектурный анализ старого QIM** - изучены все 28 entity файлов
2. **Создано 6 новых entities:**
   - ClientArea (Inländisch/EU/Ausländisch)
   - ClientType (B2B/B2C)
   - Account (Erlöskonto: 4000, 4010, 4030, 4062, 4100, 4112)
   - Tax (TaxRate + Account комбинация)
   - BankAccount (IBAN, BIC, BLZ)
   - InvoiceDiscount (Many-to-Many)

3. **Обновлены существующие entities:**
   - Client: ClientTypeId, ClientAreaId (FK вместо enum)
   - Invoice: BankAccountId, PaymentMethodId, InvoiceDiscounts
   - InvoiceItem: TaxId (FK вместо decimal TaxRate)

4. **Модульная архитектура:**
   - AR Module (Ausgangsrechnungen) - ГОТОВ
   - ER Module (Eingangsrechnungen) - Coming Soon
   - KA Module (Kassa) - Coming Soon

5. **Созданы UI страницы:**
   - AR/Clients/Index.razor (148 lines)
   - AR/Clients/CreateEdit.razor (283 lines)
   - AR/Invoices/Index.razor (143 lines)
   - AR/Invoices/CreateEdit.razor (326 lines)

6. **Обновлены сервисы:**
   - ClientService: GenerateNextClientCodeAsync (200000/230000/260000)
   - InvoiceService: работа с новыми FK

7. **База данных (SQL Server):**
   - Миграция AddAccountTaxSystem - применена ✅
   - Миграция InsertEurCurrency - применена ✅
   - Проблема: Foreign Key ошибка с Currencies

#### Часть 2: Миграция на SQLite и запуск приложения (вечер)
8. **Миграция с Azure SQL/LocalDB на SQLite:**
   - Проблема: Azure SQL недоступен (qimy-sql-server.database.windows.net)
   - Проблема: LocalDB не установлен
   - Проблема: SQL Express не найден
   - Решение: ✅ Переход на SQLite (QImyDb.db)

9. **Установка SQLite:**
   - Добавлен пакет Microsoft.EntityFrameworkCore.Sqlite 8.0.11
   - Изменены connection strings в appsettings.json
   - Изменён Program.cs: UseSqlServer → UseSqlite

10. **База данных SQLite:**
    - Удалены старые SQL Server миграции
    - Создана новая миграция InitialSQLite (20260119153851)
    - Применена успешно ✅
    - Файл БД: C:\Projects\QIMy\src\QIMy.Web\QImyDb.db

11. **Создание администратора:**
    - Установлен dotnet-script 2.0.0
    - Создан скрипт CreateUser.csx с PasswordHasher
    - Добавлен пользователь:
      - Email: office@kharitonov.at
      - Password: Admin123!
      - Id: 9cb445b8-390a-456b-b9fc-7a4ddb16d658

12. **Исправление системы входа:**
    - Проблема: NavigationException после успешного входа
    - Причина: Blazor Server требует forceLoad после SignIn
    - Решение: ✅ Добавлен параметр `forceLoad: true` в Login.razor
      `csharp
      NavigationManager.NavigateTo("/", forceLoad: true);
      `

13. **Исправление навигации:**
    - Проблема: Ссылки вели на /invoices и /clients (404 Not Found)
    - Причина: Реальные страницы в /AR/Invoices и /AR/Clients
    - Решение: ✅ Обновлены ссылки в Home.razor:
      - /invoices → /AR/Invoices
      - /clients → /AR/Clients

14. **Приложение запущено и работает:**
    - URL: http://localhost:5000 ✅
    - Вход работает ✅
    - Главная страница показывает корректные ссылки ✅
    - Страницы AR модуля доступны ✅

### 🎯 Итоговый статус:

- ✅ База данных: SQLite (QImyDb.db)
- ✅ Аутентификация: ASP.NET Core Identity работает
- ✅ Администратор создан: office@kharitonov.at
- ✅ Вход в систему: без ошибок
- ✅ Навигация: исправлена
- ✅ AR модуль: страницы клиентов и счетов работают
- ✅ Сервер запущен на localhost:5000
- ✅ Компиляция: 0 errors, 0 warnings

### 📝 Технические изменения:

**Файлы изменены:**
1. `src/QIMy.Web/appsettings.json` - SQLite connection string
2. `src/QIMy.Web/appsettings.Development.json` - SQLite connection string
3. `src/QIMy.Web/Program.cs` - UseSqlite вместо UseSqlServer
4. `src/QIMy.Web/Components/Pages/Account/Login.razor` - forceLoad при навигации
5. `src/QIMy.Web/Components/Pages/Home.razor` - правильные пути к AR модулю
6. `src/QIMy.Infrastructure/Migrations/` - InitialSQLite миграция

**Созданные скрипты:**
- `CreateUser.csx` - создание администратора с хешированием пароля
- `CheckUser.csx` - проверка пользователей в БД

### 🔧 Следующие шаги:

1. **Заполнить справочники:**
   - ClientAreas (Inländisch, EU, Ausländisch)
   - ClientTypes (B2B, B2C)
   - Taxes (с привязкой к Accounts)
   - BankAccounts
   - PaymentMethods
   - Currencies (EUR, USD, RUR)

2. **Протестировать функционал:**
   - Создание клиента
   - Создание счета
   - Расчёт налогов
   - Генерация номеров

3. **Дальнейшая разработка:**
   - PDF генерация счетов
   - Импорт клиентов из CSV (tabellen/Klienten.csv)
   - BMD Export функционал
   - ER Module (входящие счета)
   - KA Module (касса)

## Файловая структура

\\\
QIMy/
├── src/
│   ├── QIMy.Core/
│   │   └── Entities/
│   │       ├── ClientArea.cs ✅ NEW
│   │       ├── ClientType.cs ✅ NEW
│   │       ├── Account.cs ✅ NEW
│   │       ├── Tax.cs ✅ NEW
│   │       ├── BankAccount.cs ✅ NEW
│   │       ├── InvoiceDiscount.cs ✅ NEW
│   │       ├── Client.cs ✏️ UPDATED
│   │       ├── Invoice.cs ✏️ UPDATED
│   │       ├── InvoiceItem.cs ✏️ UPDATED
│   │       └── AppUser.cs (Identity)
│   │
│   ├── QIMy.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs ✏️ UPDATED
│   │   │   └── SeedData.cs
│   │   ├── Migrations/
│   │   │   └── 20260119153851_InitialSQLite.cs ✅ SQLite
│   │   └── Services/
│   │       ├── ClientService.cs ✏️ UPDATED
│   │       └── InvoiceService.cs
│   │
│   └── QIMy.Web/
│       ├── Components/Pages/
│       │   ├── Home.razor ✏️ FIXED LINKS
│       │   ├── Account/
│       │   │   ├── Login.razor ✏️ FIXED NAV
│       │   │   ├── Logout.razor
│       │   │   └── Register.razor
│       │   └── AR/ ✅ NEW MODULE
│       │       ├── Clients/
│       │       │   ├── Index.razor ✅
│       │       │   └── CreateEdit.razor ✅
│       │       └── Invoices/
│       │           ├── Index.razor ✅
│       │           └── CreateEdit.razor ✅
│       ├── QImyDb.db ✅ SQLite Database
│       ├── appsettings.json ✏️ SQLite
│       └── Program.cs ✏️ SQLite
│
├── CreateUser.csx ✅ User creation script
├── CheckUser.csx ✅ User verification script
└── tabellen/
    └── Klienten.csv (BMD export data)
\\\

## Технические детали

### Erlöskonto → Tax Rate Mappings:
- 4000 → 20% (Standard VAT - Inland)
- 4010 → 20% (Barverkauf - Cash sales)
- 4030 → 20% (Standard VAT - Inland alternative)
- 4062 → 10% (Reduced VAT)
- 4100 → 0% (Export - VAT free)
- 4112 → 13% (Special reduced VAT)

### ClientCode Ranges:
- 200000-229999: Inländisch (Inland)
- 230000-259999: EU
- 260000-299999: Ausländisch (Export/Third Countries)

### Application Status:
- ✅ Server: http://localhost:5000
- ✅ Database: SQLite (QImyDb.db)
- ✅ Admin: office@kharitonov.at / Admin123!
- ✅ Build: SUCCESS (0 errors, 0 warnings)
- ✅ Login: Working with forceLoad fix
- ✅ Navigation: Fixed AR module links

### Terminal ID для приложения:
- d3c982e9-45d7-4385-8184-28b2538edfab (запущено в фоне)

## Terminal Commands для следующей сессии:

\\\powershell
# 1. Проверить статус приложения
Get-Process | Where-Object {$_.ProcessName -eq "dotnet"}

# 2. Остановить если нужно
Stop-Process -Name "dotnet" -Force

# 3. Запустить приложение
cd C:\Projects\QIMy
dotnet run --project "C:\Projects\QIMy\src\QIMy.Web\QIMy.Web.csproj" --urls "http://localhost:5000"

# 4. Проверить пользователей в БД
dotnet script CheckUser.csx

# 5. Создать миграцию (если нужно)
dotnet ef migrations add MigrationName --project src\QIMy.Infrastructure --startup-project src\QIMy.Web

# 6. Применить миграции
dotnet ef database update --project src\QIMy.Infrastructure --startup-project src\QIMy.Web
\\\

## Проблемы и решения:

### 1. Azure SQL недоступен
- **Проблема:** qimy-sql-server.database.windows.net не отвечает
- **Решение:** Миграция на SQLite для локальной разработки

### 2. LocalDB не установлен
- **Проблема:** Unable to locate a Local Database Runtime installation
- **Решение:** Переход на SQLite

### 3. SQL Express не найден
- **Проблема:** Error Locating Server/Instance Specified [.\SQLEXPRESS]
- **Решение:** Переход на SQLite

### 4. SQL Server миграции несовместимы с SQLite
- **Проблема:** Синтаксис nvarchar(max), datetime2, bit не работает
- **Решение:** Создана новая InitialSQLite миграция

### 5. Нет тестового пользователя
- **Проблема:** Login возвращал "Неверный email или пароль"
- **Решение:** Создан скрипт CreateUser.csx с PasswordHasher

### 6. NavigationException после входа
- **Проблема:** Login успешен, но NavigationManager выбрасывает исключение
- **Решение:** Добавлен `forceLoad: true` для полной перезагрузки страницы

### 7. 404 Not Found на /invoices
- **Проблема:** Ссылки на главной странице вели на несуществующие маршруты
- **Решение:** Обновлены пути на /AR/Invoices и /AR/Clients

---
**Session Start:** 19.01.2026 02:00 CET
**Session End:** 19.01.2026 23:45 CET
**Duration:** ~10 hours (с перерывами)
**Next Session:** 20.01.2026

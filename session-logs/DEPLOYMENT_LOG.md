# QIMy - Лог Развертывания и Прогресс

**Дата**: 18 января 2026  
**Статус**: 🔄 Azure SQL Database разворачивается (5-10 минут)

---

## ✅ Что уже сделано

### 1. Установка и настройка среды
- ✅ .NET 8.0.417 SDK установлен
- ✅ EF Core CLI установлен (dotnet-ef 10.0.2)

### 2. Структура проекта создана
```
QIMy/
├── QIMy.sln
├── src/
│   ├── QIMy.Web/          (Blazor Server - точка входа)
│   ├── QIMy.API/          (Web API)
│   ├── QIMy.Core/         (Domain layer - 15 сущностей)
│   ├── QIMy.Infrastructure/ (Data access - EF Core)
│   └── QIMy.Shared/       (DTOs)
```

### 3. Domain Model (15 сущностей)
- Client (клиенты)
- Supplier (поставщики)
- Product (товары/услуги)
- Business (бизнесы/организации)
- Invoice (исходящие счета) + InvoiceItem
- Payment (платежи)
- ExpenseInvoice (входящие счета) + ExpenseInvoiceItem
- Currency (валюты)
- TaxRate (налоговые ставки)
- Unit (единицы измерения)
- PaymentMethod (способы оплаты)
- Discount (скидки)

### 4. База данных - Azure SQL
**Настройки:**
- **Сервер**: qimy-sql-server.database.windows.net
- **База данных**: QImyDb
- **Регион**: West Europe
- **Тариф**: Standard S0 (~10-12 EUR/месяц)
- **Среда**: Разработка
- **Резервные копии**: Локально избыточное хранилище
- **Ресурс группа**: QIMy-Resources

### 5. EF Core миграция создана
```bash
dotnet ef migrations add InitialCreate
```
✅ Миграция создана (11 таблиц готовы к развертыванию)  
⏳ НЕ применена (ждём строку подключения)

### 6. Конфигурация подготовлена
- ✅ appsettings.json - шаблон для Azure SQL
- ✅ appsettings.Development.json - LocalDB для локальной разработки
- ✅ appsettings.Production.json - Azure SQL для продакшена
- ✅ Program.cs - retry logic для облачной устойчивости

---

## 🔄 Текущий статус (18.01.2026, 20:17)

**Azure Portal:**  
Развертывание SQL Server начато в 20:17  
Ожидаемое время: 5-10 минут (завершится ~20:25)

**Статус:**
```
Идентификатор развертывания: b54b4b8-d462-4760-af86-e44c75108d87
Подписка: Azure subscription 1
Группа ресурсов: QIMy-Resources
Ресурс: qimy-sql-server
```

---

## ⏭️ Следующие шаги (ПОСЛЕ ВОЗВРАЩЕНИЯ)

### ШАГ 1: Получить строку подключения (5 минут)

1. Дождаться завершения развертывания в Azure Portal
2. Нажать **"Перейти к ресурсу"**
3. В левом меню найти **"Строки подключения"** или **"Connection strings"**
4. Скопировать строку **ADO.NET**
5. Строка будет выглядеть примерно так:
```
Server=tcp:qimy-sql-server.database.windows.net,1433;Initial Catalog=QImyDb;Persist Security Info=False;User ID={ваш_логин};Password={ваш_пароль};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### ШАГ 2: Настроить Firewall (ВАЖНО!)

Azure SQL по умолчанию блокирует все подключения. Нужно:

1. В Azure Portal откройте **qimy-sql-server** (сервер, не база!)
2. Слева найдите **"Networking"** или **"Сети"**
3. Добавьте правило:
   - **Имя**: AllowMyIP
   - Нажмите **"Add your client IPv4 address"** (добавить мой IP)
   - **СОХРАНИТЕ!**

Или временно (только для разработки):
- Включите **"Allow Azure services and resources to access this server"**

### ШАГ 3: Обновить конфигурацию (скажите мне строку подключения)

Отправьте мне строку подключения, и я выполню:

```bash
# Обновлю файлы конфигурации с реальной строкой
# Файлы для обновления:
# - src/QIMy.Web/appsettings.json
# - src/QIMy.Web/appsettings.Production.json
```

### ШАГ 4: Применить миграции (я выполню)

```bash
cd C:\Projects\QIMy
dotnet ef database update --project src/QIMy.Infrastructure --startup-project src/QIMy.Web
```

Это создаст все таблицы в Azure SQL:
- Clients
- Suppliers
- Products
- Businesses
- Invoices
- InvoiceItems
- Payments
- ExpenseInvoices
- ExpenseInvoiceItems
- Currencies
- TaxRates
- Units
- PaymentMethods
- Discounts

### ШАГ 5: Проверить подключение

```bash
dotnet run --project src/QIMy.Web
```

Приложение запустится на https://localhost:5001

---

## 📋 Дальнейший план разработки

### День 1-2: ASP.NET Core Identity
- [ ] Установить Microsoft.AspNetCore.Identity.EntityFrameworkCore
- [ ] Создать AppUser : IdentityUser
- [ ] Добавить BusinessId для мультитенантности
- [ ] Обновить ApplicationDbContext
- [ ] Создать миграцию для Identity
- [ ] Настроить регистрацию/логин

### День 2-3: Базовый UI (Blazor)
- [ ] MainLayout с навигацией
- [ ] Страница Dashboard с KPI
- [ ] CRUD для Клиентов
- [ ] CRUD для Счетов (Invoice)
- [ ] CRUD для Входящих счетов (ExpenseInvoice)

### День 4-5: Бизнес-логика
- [ ] Сервисы для работы с Invoice
- [ ] PDF генерация счетов
- [ ] Email отправка
- [ ] Расчёт налогов (UVA для Австрии)

### День 6-7: Azure Deployment
- [ ] Создать Azure App Service
- [ ] Настроить CI/CD (GitHub Actions или Azure DevOps)
- [ ] Настроить Azure Blob Storage для PDF
- [ ] Настроить HTTPS и custom domain
- [ ] Тестирование с мобильных устройств

---

## 🛠️ Техническая информация

### Пакеты установлены
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
```

### Команды для работы с EF Core
```bash
# Создать новую миграцию
dotnet ef migrations add MigrationName --project src/QIMy.Infrastructure --startup-project src/QIMy.Web

# Применить миграции
dotnet ef database update --project src/QIMy.Infrastructure --startup-project src/QIMy.Web

# Откатить миграцию
dotnet ef database update PreviousMigration --project src/QIMy.Infrastructure --startup-project src/QIMy.Web

# Удалить последнюю миграцию
dotnet ef migrations remove --project src/QIMy.Infrastructure --startup-project src/QIMy.Web
```

### Команды для запуска
```bash
# Запуск Web приложения
dotnet run --project src/QIMy.Web

# Запуск API
dotnet run --project src/QIMy.API

# Сборка всего решения
dotnet build

# Публикация для Azure
dotnet publish src/QIMy.Web -c Release -o publish
```

---

## 🔐 Безопасность (TODO после миграции)

### Строка подключения НЕ должна быть в Git!
После получения реальной строки:
1. Добавить в `.gitignore`:
```
appsettings.Production.json
appsettings.Development.json
**/appsettings.*.json
```

2. Использовать User Secrets для разработки:
```bash
dotnet user-secrets init --project src/QIMy.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "строка_подключения" --project src/QIMy.Web
```

3. В Azure App Service хранить строку в Configuration → Connection strings

---

## 📞 Контакты и ссылки

**Azure Portal**: https://portal.azure.com  
**Оригинальный QIM**: https://qim.azurewebsites.net  
**Документация .NET**: https://docs.microsoft.com/dotnet  
**Документация EF Core**: https://docs.microsoft.com/ef/core  

---

## ⚠️ Важные замечания

1. **Firewall Azure SQL**: Обязательно добавить свой IP в Networking!
2. **Стоимость**: Standard S0 = ~10-12 EUR/месяц (отслеживайте в Azure Cost Management)
3. **Backup**: Резервные копии автоматические (7 дней хранения для Standard)
4. **LocalDB**: Для локальной разработки оффлайн можно установить SQL Server Express или LocalDB

---

**Последнее обновление**: 18.01.2026, 20:17  
**Следующий шаг**: Дождаться завершения развертывания Azure SQL → Получить строку подключения

---

## ✅ МИГРАЦИЯ УСПЕШНО ПРИМЕНЕНА!

**Дата и время**: 18.01.2026, 23:10

### Созданные таблицы в Azure SQL:
1. ✅ Businesses - организации
2. ✅ Clients - клиенты  
3. ✅ Suppliers - поставщики
4. ✅ Products - товары/услуги
5. ✅ Invoices - исходящие счета
6. ✅ InvoiceItems - позиции счетов
7. ✅ ExpenseInvoices - входящие счета
8. ✅ ExpenseInvoiceItems - позиции входящих счетов
9. ✅ Payments - платежи
10. ✅ Currencies - валюты
11. ✅ TaxRates - налоговые ставки
12. ✅ Units - единицы измерения
13. ✅ PaymentMethods - способы оплаты
14. ✅ Discounts - скидки
15. ✅ __EFMigrationsHistory - история миграций

### Подключение:
**Сервер**: qimy-sql-server.database.windows.net
**База данных**: QImyDb
**Статус**: ✅ Активно и доступно

### Следующие шаги:
1. ⏳ Добавить ASP.NET Core Identity для аутентификации
2. ⏳ Создать базовые страницы Blazor UI
3. ⏳ Реализовать бизнес-логику (сервисы)
4. ⏳ Развернуть в Azure App Service


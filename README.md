# QIMy - Modern Cloud Accounting Software

## 🎯 Описание

QIMy - современная облачная система бухгалтерского учёта нового поколения, построенная на .NET 8 и Blazor.

### ✨ Ключевые функции

- **AR (Ausgangsrechnungen)** - Управление исходящими счетами
- **ER (Eingangsrechnungen)** - Управление входящими счетами с OCR
- **Registrierkasse** - Кассовый модуль
- **Banking Integration** - Интеграция с банками
- **Multi-language** - Поддержка DE/EN/RU
- **Real-time Dashboard** - Аналитика в реальном времени

## 🏗️ Архитектура

```
QIMy/
├── src/
│   ├── QIMy.Web/              # Blazor Web UI
│   ├── QIMy.API/              # REST API
│   ├── QIMy.Core/             # Domain Models & Business Logic
│   ├── QIMy.Infrastructure/   # Data Access & External Services
│   └── QIMy.Shared/           # Shared DTOs & Utilities
├── tests/
└── docs/
```

## 🚀 Технологический стек

- **.NET 8** - Backend framework
- **Blazor** - Frontend (C# + HTML/CSS)
- **Entity Framework Core** - ORM
- **PostgreSQL / SQL Server** - Database
- **ASP.NET Core Identity** - Authentication
- **SignalR** - Real-time updates

## 📦 Установка

### Требования
- .NET 8 SDK
- SQL Server или PostgreSQL
- Visual Studio 2022 / VS Code / Rider

### Запуск

```bash
# Восстановление зависимостей
dotnet restore

# Запуск веб-приложения
cd src/QIMy.Web
dotnet run

# Запуск API
cd src/QIMy.API
dotnet run
```

## 🗄️ База данных

```bash
# Создание миграции
dotnet ef migrations add InitialCreate -p src/QIMy.Infrastructure -s src/QIMy.Web

# Применение миграций
dotnet ef database update -p src/QIMy.Infrastructure -s src/QIMy.Web
```

## 📝 Лицензия

Proprietary - All rights reserved

## 👥 Команда

Разработка: QIM Team  
Год: 2026

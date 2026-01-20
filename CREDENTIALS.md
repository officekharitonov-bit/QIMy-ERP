# QIMy - Учётные данные и пароли

## Azure SQL Server
- **Server:** qimy-sql-server.database.windows.net
- **Database:** QImyDb
- **Admin Login:** qimyadmin
- **Password:** h970334054CRgd1!
- **Connection String:**
```
Server=tcp:qimy-sql-server.database.windows.net,1433;Initial Catalog=QImyDb;Persist Security Info=False;User ID=qimyadmin;Password=h970334054CRgd1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Администратор приложения
- **Email:** office@kharitonov.at
- **Password:** Admin123!
- **Создан:** автоматически через SeedData при старте приложения

## Azure Web App
- **URL:** https://qimy-erp-app.azurewebsites.net
- **Name:** qimy-erp-app
- **Region:** West Europe

## GitHub Repository
- **URL:** https://github.com/officekharitonov-bit/QIMy-ERP.git
- **Branch:** main
- **Auto Deploy:** GitHub Actions → Azure Web App

## Локальная разработка
- **Database:** SQLite (QImyDb.db)
- **Connection String:** Data Source=QImyDb.db
- **URL:** http://localhost:5000

## Важные файлы
- `appsettings.json` - SQLite для Development
- `appsettings.Production.json` - Azure SQL для Production
- `Program.cs` - автоматический выбор провайдера БД по окружению

---
**ВНИМАНИЕ:** Храните этот файл в безопасности! Добавьте в .gitignore если нужно!

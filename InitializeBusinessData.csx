#!/usr/bin/env dotnet-script
#nullable enable
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System;

var dbPath = "src/QIMy.Web/QImyDb.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("=== Инициализация BusinessId для всех данных ===\n");

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    // 1. Проверяем/создаем бизнес
    var getBusinessCmd = connection.CreateCommand();
    getBusinessCmd.CommandText = "SELECT Id, Name FROM Businesses ORDER BY Id LIMIT 1";

    int businessId;
    string? businessName = null;

    using (var reader = getBusinessCmd.ExecuteReader())
    {
        if (reader.Read())
        {
            businessId = reader.GetInt32(0);
            businessName = reader.GetString(1);
            Console.WriteLine($"✓ Найден бизнес: ID={businessId}, Name='{businessName}'");
        }
        else
        {
            reader.Close();

            // Создаем тестовый бизнес
            var createBusinessCmd = connection.CreateCommand();
            createBusinessCmd.CommandText = @"
                INSERT INTO Businesses (Name, LegalName, Email, CreatedAt, IsDeleted)
                VALUES (@name, @legalName, @email, @createdAt, 0);
                SELECT last_insert_rowid();";
            createBusinessCmd.Parameters.AddWithValue("@name", "Тестовая компания");
            createBusinessCmd.Parameters.AddWithValue("@legalName", "ООО Тестовая компания");
            createBusinessCmd.Parameters.AddWithValue("@email", "test@company.com");
            createBusinessCmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));

            businessId = Convert.ToInt32(createBusinessCmd.ExecuteScalar());
            businessName = "Тестовая компания";
            Console.WriteLine($"✓ Создан новый бизнес: ID={businessId}, Name='{businessName}'");
        }
    }

    Console.WriteLine();

    // 2. Обновляем все таблицы
    var tablesToUpdate = new[]
    {
        "ClientAreas",
        "ClientTypes",
        "TaxRates",
        "Units",
        "Currencies",
        "PaymentMethods",
        "Discounts",
        "Products",
        "Suppliers",
        "Accounts",
        "NumberingConfigs",
        "Clients",
        "Invoices",
        "ExpenseInvoices",
        "Quotes",
        "Returns",
        "DeliveryNotes",
        "Taxes",
        "Payments"
    };

    foreach (var table in tablesToUpdate)
    {
        try
        {
            // Проверяем, существует ли таблица
            var checkTableCmd = connection.CreateCommand();
            checkTableCmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'";
            var tableExists = checkTableCmd.ExecuteScalar() != null;

            if (!tableExists)
            {
                Console.WriteLine($"⊗ Таблица {table} не существует - пропускаем");
                continue;
            }

            // Считаем записи без BusinessId
            var countCmd = connection.CreateCommand();
            countCmd.CommandText = $"SELECT COUNT(*) FROM {table} WHERE BusinessId IS NULL";
            var nullCount = (long)(countCmd.ExecuteScalar() ?? 0L);

            if (nullCount == 0)
            {
                Console.WriteLine($"✓ {table}: все записи уже имеют BusinessId");
                continue;
            }

            // Обновляем
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = $@"
                UPDATE {table}
                SET BusinessId = @businessId
                WHERE BusinessId IS NULL";
            updateCmd.Parameters.AddWithValue("@businessId", businessId);

            var updated = updateCmd.ExecuteNonQuery();
            Console.WriteLine($"✓ {table}: обновлено {updated} записей");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ {table}: ошибка - {ex.Message}");
        }
    }

    Console.WriteLine("\n=== Проверка результатов ===\n");

    // 3. Проверяем результаты
    foreach (var table in tablesToUpdate)
    {
        try
        {
            var checkTableCmd = connection.CreateCommand();
            checkTableCmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'";
            var tableExists = checkTableCmd.ExecuteScalar() != null;

            if (!tableExists) continue;

            var totalCmd = connection.CreateCommand();
            totalCmd.CommandText = $"SELECT COUNT(*) FROM {table} WHERE IsDeleted = 0";
            var total = (long)(totalCmd.ExecuteScalar() ?? 0L);

            var withBusinessCmd = connection.CreateCommand();
            withBusinessCmd.CommandText = $"SELECT COUNT(*) FROM {table} WHERE BusinessId = {businessId} AND IsDeleted = 0";
            var withBusiness = (long)(withBusinessCmd.ExecuteScalar() ?? 0L);

            var nullCmd = connection.CreateCommand();
            nullCmd.CommandText = $"SELECT COUNT(*) FROM {table} WHERE BusinessId IS NULL AND IsDeleted = 0";
            var nulls = (long)(nullCmd.ExecuteScalar() ?? 0L);

            if (total > 0)
            {
                var status = nulls == 0 ? "✓" : "⚠";
                Console.WriteLine($"{status} {table,-20} Всего: {total,3}, с BusinessId: {withBusiness,3}, без: {nulls,3}");
            }
        }
        catch { }
    }

    Console.WriteLine($"\n✅ Инициализация завершена! Все данные привязаны к бизнесу ID={businessId}");
}

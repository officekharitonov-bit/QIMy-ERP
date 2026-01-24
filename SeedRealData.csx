#!/usr/bin/env dotnet-script
#nullable enable
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System;

var dbPath = "QIMy.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("=== ИНИЦИАЛИЗАЦИЯ РЕАЛЬНЫХ ДАННЫХ ===\n");

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    var transaction = connection.BeginTransaction();

    try
    {
        // 1. Создаём бизнес если его нет
        var getBusinessCmd = connection.CreateCommand();
        getBusinessCmd.CommandText = "SELECT Id FROM Businesses LIMIT 1";
        var businessId = 1;

        var existingBusiness = getBusinessCmd.ExecuteScalar();
        if (existingBusiness == null)
        {
            var createCmd = connection.CreateCommand();
            createCmd.CommandText = @"
                INSERT INTO Businesses (Name, LegalName, Email, CreatedAt, IsDeleted)
                VALUES ('Mag. Kharitonov Egor', 'Mag. Kharitonov Egor', 'office@kharitonov.at', @date, 0)";
            createCmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("o"));
            createCmd.ExecuteNonQuery();
            var lastCmd = connection.CreateCommand();
            lastCmd.CommandText = "SELECT last_insert_rowid()";
            businessId = Convert.ToInt32(lastCmd.ExecuteScalar());
            Console.WriteLine($"✓ Создан бизнес ID={businessId}");
        }
        else
        {
            businessId = Convert.ToInt32(existingBusiness);
            Console.WriteLine($"✓ Бизнес найден ID={businessId}");
        }

        // 2. Добавляем клиентов если их нет
        var clientCountCmd = connection.CreateCommand();
        clientCountCmd.CommandText = "SELECT COUNT(*) FROM Clients WHERE IsDeleted = 0";
        var clientCount = (long)(clientCountCmd.ExecuteScalar() ?? 0L);

        if (clientCount == 0)
        {
            Console.WriteLine("\n↳ Добавляем клиентов...");
            var clients = new[]
            {
                ("Mag. Kharitonov Egor", "AT"),
                ("AKHA GmbH", "AT"),
                ("VKNA GmbH", "AT"),
                ("Test Client 1", "AT"),
                ("Test Client 2", "DE"),
            };

            foreach (var (name, country) in clients)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Clients (Name, CountryCode, BusinessId, CreatedAt, IsDeleted)
                    VALUES (@name, @country, @businessId, @date, 0)";
                insertCmd.Parameters.AddWithValue("@name", name);
                insertCmd.Parameters.AddWithValue("@country", country);
                insertCmd.Parameters.AddWithValue("@businessId", businessId);
                insertCmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("o"));
                insertCmd.ExecuteNonQuery();
                Console.WriteLine($"  ✓ Клиент: {name}");
            }
        }
        else
        {
            Console.WriteLine($"✓ Клиентов уже есть: {clientCount}");
        }

        // 3. Проверяем поставщиков
        var supplierCountCmd = connection.CreateCommand();
        supplierCountCmd.CommandText = "SELECT COUNT(*) FROM Suppliers WHERE IsDeleted = 0";
        var supplierCount = (long)(supplierCountCmd.ExecuteScalar() ?? 0L);

        if (supplierCount == 0)
        {
            Console.WriteLine("\n↳ Добавляем поставщиков...");
            var suppliers = new[]
            {
                ("TestSupplier1", "AT"),
                ("TestSupplier2", "DE"),
                ("TestSupplier3", "CH"),
            };

            foreach (var (name, country) in suppliers)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Suppliers (Name, CountryCode, BusinessId, CreatedAt, IsDeleted)
                    VALUES (@name, @country, @businessId, @date, 0)";
                insertCmd.Parameters.AddWithValue("@name", name);
                insertCmd.Parameters.AddWithValue("@country", country);
                insertCmd.Parameters.AddWithValue("@businessId", businessId);
                insertCmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("o"));
                insertCmd.ExecuteNonQuery();
                Console.WriteLine($"  ✓ Поставщик: {name}");
            }
        }
        else
        {
            Console.WriteLine($"✓ Поставщиков уже есть: {supplierCount}");
        }

        // 4. Проверяем счета AR
        var invoiceCountCmd = connection.CreateCommand();
        invoiceCountCmd.CommandText = "SELECT COUNT(*) FROM Invoices WHERE IsDeleted = 0";
        var invoiceCount = (long)(invoiceCountCmd.ExecuteScalar() ?? 0L);

        Console.WriteLine($"\n✓ Счета AR (исходящие): {invoiceCount}");

        // 5. Проверяем счета ER
        var expenseCountCmd = connection.CreateCommand();
        expenseCountCmd.CommandText = "SELECT COUNT(*) FROM ExpenseInvoices WHERE IsDeleted = 0";
        var expenseCount = (long)(expenseCountCmd.ExecuteScalar() ?? 0L);

        Console.WriteLine($"✓ Счета ER (входящие): {expenseCount}");

        transaction.Commit();
        Console.WriteLine($"\n✅ ИНИЦИАЛИЗАЦИЯ ЗАВЕРШЕНА!");
        Console.WriteLine($"  • Бизнес: {businessId}");
        Console.WriteLine($"  • Клиентов инициализировано");
        Console.WriteLine($"  • Поставщиков инициализировано");
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        Console.WriteLine($"\n❌ ОШИБКА: {ex.Message}");
        throw;
    }
}

#!/usr/bin/env dotnet-script
#nullable enable
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System;

var dbPath = "src/QIMy.API/QImyDb.db";
var connectionString = $"Data Source={dbPath}";

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    
    // Получаем первый бизнес
    var getBusinessCmd = connection.CreateCommand();
    getBusinessCmd.CommandText = "SELECT Id, Name FROM Businesses ORDER BY Id LIMIT 1";
    
    int? firstBusinessId = null;
    string? businessName = null;
    
    using (var reader = getBusinessCmd.ExecuteReader())
    {
        if (reader.Read())
        {
            firstBusinessId = reader.GetInt32(0);
            businessName = reader.GetString(1);
            Console.WriteLine($"Найден бизнес: ID={firstBusinessId}, Name={businessName}");
        }
    }
    
    if (firstBusinessId.HasValue)
    {
        // Обновляем всех клиентов без BusinessId
        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE Clients 
            SET BusinessId = @businessId 
            WHERE BusinessId IS NULL";
        updateCmd.Parameters.AddWithValue("@businessId", firstBusinessId.Value);
        
        var updatedCount = updateCmd.ExecuteNonQuery();
        Console.WriteLine($"Обновлено клиентов: {updatedCount}");
        
        // Проверяем результат
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Clients WHERE BusinessId IS NULL";
        var nullCount = (long)(checkCmd.ExecuteScalar() ?? 0L);
        Console.WriteLine($"Клиентов без BusinessId: {nullCount}");
        
        checkCmd.CommandText = $"SELECT COUNT(*) FROM Clients WHERE BusinessId = {firstBusinessId}";
        var withBusinessCount = (long)(checkCmd.ExecuteScalar() ?? 0L);
        Console.WriteLine($"Клиентов с BusinessId={firstBusinessId}: {withBusinessCount}");
    }
    else
    {
        Console.WriteLine("ОШИБКА: Не найдено ни одного бизнеса в базе данных!");
    }
}

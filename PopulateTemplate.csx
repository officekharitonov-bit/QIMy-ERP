#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System.IO;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "qimy_dev.db");
Console.WriteLine($"📂 Database: {dbPath}");

if (!File.Exists(dbPath))
{
    Console.WriteLine("❌ Database file not found!");
    return 1;
}

var sqlScript = File.ReadAllText("PopulateTemplateBusiness.sql");
var connectionString = $"Data Source={dbPath}";

using var connection = new SqliteConnection(connectionString);
connection.Open();

using var transaction = connection.BeginTransaction();
try
{
    // Split by semicolon and execute each statement
    var statements = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);

    foreach (var statement in statements)
    {
        var trimmed = statement.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) continue;

        using var command = connection.CreateCommand();
        command.CommandText = trimmed;
        command.Transaction = transaction;

        if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.WriteLine($"{reader.GetName(i)}: {reader.GetValue(i)}");
                }
            }
        }
        else
        {
            var affected = command.ExecuteNonQuery();
            Console.WriteLine($"✅ Executed: {affected} rows affected");
        }
    }

    transaction.Commit();
    Console.WriteLine("✅ Template business populated successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    transaction.Rollback();
    return 1;
}

return 0;

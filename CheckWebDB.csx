#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;

var dbPath = @"src\QIMy.Web\QImyDb.db";
Console.WriteLine($"Проверка БД: {dbPath}");

var con = new SqliteConnection($"Data Source={dbPath}");
con.Open();

// Список всех таблиц
var cmd = con.CreateCommand();
cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
var reader = cmd.ExecuteReader();

var tables = new List<string>();
while (reader.Read())
{
    tables.Add(reader.GetString(0));
}

Console.WriteLine($"\n✅ Всего таблиц: {tables.Count}");
Console.WriteLine("\n📋 Первые 20 таблиц:");
foreach (var table in tables.Take(20))
{
    Console.WriteLine($"  - {table}");
}

// Проверим критичные таблицы
var criticalTables = new[] { "Businesses", "Clients", "Suppliers", "Invoices", "ExpenseInvoices", "Products" };
Console.WriteLine("\n🔍 Проверка критичных таблиц:");
foreach (var table in criticalTables)
{
    if (tables.Contains(table))
    {
        var countCmd = con.CreateCommand();
        countCmd.CommandText = $"SELECT COUNT(*) FROM {table}";
        var count = Convert.ToInt32(countCmd.ExecuteScalar());
        Console.WriteLine($"  ✅ {table}: {count} записей");
    }
    else
    {
        Console.WriteLine($"  ❌ {table}: НЕ НАЙДЕНА");
    }
}

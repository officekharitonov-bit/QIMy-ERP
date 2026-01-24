#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;

var apiDbPath = @"src\QIMy.API\QImyDb.db";
Console.WriteLine($"Проверка API БД: {apiDbPath}");

var con = new SqliteConnection($"Data Source={apiDbPath}");
con.Open();

// Проверим поставщиков
var cmd = con.CreateCommand();
cmd.CommandText = "SELECT COUNT(*) FROM Suppliers WHERE IsDeleted = 0";
var count = Convert.ToInt32(cmd.ExecuteScalar());
Console.WriteLine($"\n✅ Поставщиков в API БД: {count}");

if (count > 0)
{
    cmd.CommandText = "SELECT Id, Name, CountryCode, Email FROM Suppliers WHERE IsDeleted = 0 LIMIT 10";
    var reader = cmd.ExecuteReader();
    Console.WriteLine("\n📋 Первые 10 поставщиков:");
    while (reader.Read())
    {
        var id = reader.GetInt32(0);
        var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
        var country = reader.IsDBNull(2) ? "" : reader.GetString(2);
        var email = reader.IsDBNull(3) ? "" : reader.GetString(3);
        Console.WriteLine($"  {id}. {name} ({country}) - {email}");
    }
}

con.Close();

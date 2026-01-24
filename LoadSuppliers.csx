#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.Sqlite, 8.0.0"

using Microsoft.Data.Sqlite;
using System;

var dbPath = @"src\QIMy.Web\QImyDb.db";
Console.WriteLine($"Загрузка suppliers в: {dbPath}");

var con = new SqliteConnection($"Data Source={dbPath}");
con.Open();

// Получить BusinessId для AKHA GmbH
var getBusinessCmd = con.CreateCommand();
getBusinessCmd.CommandText = "SELECT Id FROM Businesses WHERE Name LIKE '%AKHA%' LIMIT 1";
var businessId = Convert.ToInt32(getBusinessCmd.ExecuteScalar());
Console.WriteLine($"BusinessId: {businessId}");

// Проверить текущее количество
var countCmd = con.CreateCommand();
countCmd.CommandText = "SELECT COUNT(*) FROM Suppliers WHERE IsDeleted = 0";
var currentCount = Convert.ToInt32(countCmd.ExecuteScalar());
Console.WriteLine($"Текущее количество suppliers: {currentCount}");

// Добавить тестовых suppliers
var suppliers = new[]
{
    ("Telekom Austria AG", "AT", "office@telekom.at", "ATU12345678"),
    ("Deutsche Post AG", "DE", "info@deutschepost.de", "DE123456789"),
    ("Swisscom AG", "CH", "contact@swisscom.ch", "CHE-123.456.789"),
    ("OMV AG", "AT", "office@omv.at", "ATU23456789"),
    ("Vodafone GmbH", "DE", "service@vodafone.de", "DE234567890"),
    ("Amazon EU Sarl", "LU", "business@amazon.lu", "LU12345678"),
    ("Microsoft Ireland", "IE", "info@microsoft.ie", "IE1234567X"),
    ("A1 Telekom Austria AG", "AT", "kundenservice@a1.at", "ATU34567890"),
    ("Energie AG Oberösterreich", "AT", "service@energieag.at", "ATU45678901"),
    ("Post AG", "AT", "service@post.at", "ATU56789012")
};

int added = 0;
foreach (var (name, country, email, vat) in suppliers)
{
    var insertCmd = con.CreateCommand();
    insertCmd.CommandText = @"
        INSERT INTO Suppliers (Name, CountryCode, Email, VatNumber, BusinessId, CreatedAt, UpdatedAt, IsDeleted)
        VALUES (@name, @country, @email, @vat, @businessId, @now, @now, 0)
    ";
    insertCmd.Parameters.AddWithValue("@name", name);
    insertCmd.Parameters.AddWithValue("@country", country);
    insertCmd.Parameters.AddWithValue("@email", email);
    insertCmd.Parameters.AddWithValue("@vat", vat);
    insertCmd.Parameters.AddWithValue("@businessId", businessId);
    insertCmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
    
    insertCmd.ExecuteNonQuery();
    added++;
    Console.WriteLine($"✓ Добавлен: {name} ({country})");
}

// Проверить итоговое количество
countCmd.CommandText = "SELECT COUNT(*) FROM Suppliers WHERE IsDeleted = 0";
var finalCount = Convert.ToInt32(countCmd.ExecuteScalar());

con.Close();

Console.WriteLine($"\n✅ Загрузка завершена!");
Console.WriteLine($"Добавлено: {added}");
Console.WriteLine($"Всего suppliers: {finalCount}");

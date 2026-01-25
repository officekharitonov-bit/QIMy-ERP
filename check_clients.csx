#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

var dbPath = @"c:\Projects\QIMy\src\QIMy.Web\QImyDb.db";
var connectionString = $"Data Source={dbPath}";

Console.WriteLine($"Database path: {dbPath}");
Console.WriteLine($"Database exists: {File.Exists(dbPath)}");

// Query directly
Console.WriteLine("\n--- Checking clients directly ---");
try
{
    var optionsBuilder = new DbContextOptionsBuilder();
    optionsBuilder.UseSqlite(connectionString);
    
    // Try to query directly without using the full EF context
    var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM Clients;";
    var result = command.ExecuteScalar();
    Console.WriteLine($"Total Clients: {result}");
    
    command.CommandText = "SELECT CompanyName FROM Clients LIMIT 5;";
    var reader = command.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine($"  - {reader[0]}");
    }
    connection.Close();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"

using Microsoft.EntityFrameworkCore;
using System.IO;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "qimy_dev.db");
Console.WriteLine($"📂 Database: {dbPath}");

var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

using var context = new DbContext(optionsBuilder.Options);

var sql = File.ReadAllText("PopulateTemplateBusiness.sql");

// Split by semicolon and execute
var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
    .Where(s => !string.IsNullOrWhiteSpace(s))
    .Select(s => s.Trim())
    .Where(s => !s.StartsWith("--"));

foreach (var statement in statements)
{
    try
    {
        var affected = context.Database.ExecuteSqlRaw(statement);
        Console.WriteLine($"✅ {affected} rows affected");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ {ex.Message}");
    }
}

Console.WriteLine("✅ Done!");

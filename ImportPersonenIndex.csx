#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Design, 8.0.0"
#r "nuget: ClosedXML, 0.102.2"
#r "src/QIMy.Core/bin/Debug/net8.0/QIMy.Core.dll"
#r "src/QIMy.Infrastructure/bin/Debug/net8.0/QIMy.Infrastructure.dll"

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QIMy.Infrastructure.Data;
using QIMy.Infrastructure.Services;

// Connection string for SQLite database
var connectionString = "Data Source=qimy.db";

// Excel file path
var excelPath = @"c:\Projects\QIMy\tabellen\Personen index.xlsx";

if (!File.Exists(excelPath))
{
    Console.WriteLine($"ERROR: Excel file not found: {excelPath}");
    return 1;
}

Console.WriteLine("=== Personen Index Import ===");
Console.WriteLine($"Excel file: {excelPath}");
Console.WriteLine($"Database: {connectionString}");
Console.WriteLine();

// Configure DbContext
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlite(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

// Apply migrations
Console.WriteLine("Applying migrations...");
await context.Database.MigrateAsync();
Console.WriteLine("✓ Migrations applied");
Console.WriteLine();

// Create import service
var importService = new PersonenIndexImportService(context);

// Import data
Console.WriteLine("Starting import...");
var result = await importService.ImportFromExcelAsync(excelPath);

// Display results
Console.WriteLine();
Console.WriteLine("=== Import Results ===");
Console.WriteLine($"Countries Imported: {result.CountriesImported}");
Console.WriteLine($"Countries Updated: {result.CountriesUpdated}");
Console.WriteLine($"EU Data Imported: {result.EuDataImported}");
Console.WriteLine($"EU Data Updated: {result.EuDataUpdated}");
Console.WriteLine();

if (result.Errors.Any())
{
    Console.WriteLine("ERRORS:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  ❌ {error}");
    }
    return 1;
}

Console.WriteLine("✓ Import completed successfully!");
Console.WriteLine();
Console.WriteLine(result.Summary);

return 0;

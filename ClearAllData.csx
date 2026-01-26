#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"

using Microsoft.EntityFrameworkCore;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "QIMy.Web", "QImyDb.db");
var connectionString = $"Data Source={dbPath}";

var optionsBuilder = new DbContextOptionsBuilder();
optionsBuilder.UseSqlite(connectionString);

var context = new ClearDataContext(optionsBuilder.Options);

Console.WriteLine("=== CLEARING ALL DATA ===\n");

try
{
    // Delete all invoices first (they have foreign keys to clients and products)
    Console.WriteLine("Deleting all invoices...");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Invoices");
    Console.WriteLine("✅ All invoices deleted\n");

    // Delete all clients
    Console.WriteLine("Deleting all clients...");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Clients");
    Console.WriteLine("✅ All clients deleted\n");

    // Delete all products/services
    Console.WriteLine("Deleting all products...");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Products");
    Console.WriteLine("✅ All products deleted\n");

    Console.WriteLine("=== DATA CLEARED SUCCESSFULLY ===");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    throw;
}

public class ClearDataContext : DbContext
{
    public ClearDataContext(DbContextOptions options) : base(options) { }
}

#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.11"
#r "nuget: Microsoft.Extensions.Configuration, 8.0.0"
#r "nuget: Microsoft.Extensions.Configuration.Json, 8.0.0"

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "src", "QIMy.Web"))
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection String: {connectionString}");

var optionsBuilder = new DbContextOptionsBuilder();
optionsBuilder.UseSqlite(connectionString);

// Load assemblies
var infrastructureAssembly = System.Reflection.Assembly.LoadFrom(
    Path.Combine(Directory.GetCurrentDirectory(), "src", "QIMy.Infrastructure", "bin", "Debug", "net8.0", "QIMy.Infrastructure.dll")
);

var contextType = infrastructureAssembly.GetType("QIMy.Infrastructure.Data.ApplicationDbContext");
var seedDataType = infrastructureAssembly.GetType("QIMy.Infrastructure.Data.SeedData");

if (contextType == null || seedDataType == null)
{
    Console.WriteLine("ERROR: Could not load ApplicationDbContext or SeedData types");
    return;
}

dynamic context = Activator.CreateInstance(contextType, optionsBuilder.Options);

Console.WriteLine("\n🌱 Running Seed Data...\n");

var seedMethod = seedDataType.GetMethod("SeedReferenceData");
await (Task)seedMethod.Invoke(null, new[] { context });

Console.WriteLine("\n✅ Seed Data Complete!\n");

// Verify data
Console.WriteLine("📊 Data Statistics:");
Console.WriteLine($"  ClientAreas: {await context.ClientAreas.CountAsync()}");
Console.WriteLine($"  ClientTypes: {await context.ClientTypes.CountAsync()}");
Console.WriteLine($"  TaxRates: {await context.TaxRates.CountAsync()}");
Console.WriteLine($"  Accounts: {await context.Accounts.CountAsync()}");
Console.WriteLine($"  Taxes: {await context.Taxes.CountAsync()}");
Console.WriteLine($"  Units: {await context.Units.CountAsync()}");
Console.WriteLine($"  Currencies: {await context.Currencies.CountAsync()}");
Console.WriteLine($"  PaymentMethods: {await context.PaymentMethods.CountAsync()}");

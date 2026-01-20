using Microsoft.EntityFrameworkCore;
using QIMy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("src/QIMy.Web/appsettings.json")
    .Build();

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite(config.GetConnectionString("DefaultConnection"))
    .Options;

using var context = new ApplicationDbContext(options);

Console.WriteLine("Starting SeedData...\n");

await SeedData.SeedReferenceData(context);

Console.WriteLine("\n=== Seed Complete ===\n");

// Display results
Console.WriteLine($"ClientAreas: {context.ClientAreas.Count()}");
Console.WriteLine($"ClientTypes: {context.ClientTypes.Count()}");
Console.WriteLine($"TaxRates: {context.TaxRates.Count()}");
Console.WriteLine($"Accounts: {context.Accounts.Count()}");
Console.WriteLine($"Taxes: {context.Taxes.Count()}");
Console.WriteLine($"Units: {context.Units.Count()}");
Console.WriteLine($"Currencies: {context.Currencies.Count()}");
Console.WriteLine($"Clients: {context.Clients.Count()}");

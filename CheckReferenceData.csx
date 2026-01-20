using Microsoft.EntityFrameworkCore;
using QIMy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/QIMy.Web/appsettings.json")
    .Build();

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));

using var context = new ApplicationDbContext(optionsBuilder.Options);

Console.WriteLine("=== Текущие справочники ===\n");

var clientAreas = await context.ClientAreas.ToListAsync();
Console.WriteLine($"ClientAreas: {clientAreas.Count}");
foreach (var ca in clientAreas)
{
    Console.WriteLine($"  - {ca.Name} (Code: {ca.Code})");
}

var clientTypes = await context.ClientTypes.ToListAsync();
Console.WriteLine($"\nClientTypes: {clientTypes.Count}");
foreach (var ct in clientTypes)
{
    Console.WriteLine($"  - {ct.Name} (Code: {ct.Code})");
}

var accounts = await context.Accounts.ToListAsync();
Console.WriteLine($"\nAccounts: {accounts.Count}");
foreach (var acc in accounts)
{
    Console.WriteLine($"  - {acc.AccountNumber}: {acc.Name}");
}

var taxRates = await context.TaxRates.ToListAsync();
Console.WriteLine($"\nTaxRates: {taxRates.Count}");
foreach (var tr in taxRates)
{
    Console.WriteLine($"  - {tr.Name}: {tr.Rate}%");
}

var currencies = await context.Currencies.ToListAsync();
Console.WriteLine($"\nCurrencies: {currencies.Count}");
foreach (var cur in currencies)
{
    Console.WriteLine($"  - {cur.Code}: {cur.Name}");
}

Console.WriteLine($"\n=== Всего клиентов: {await context.Clients.CountAsync()} ===");

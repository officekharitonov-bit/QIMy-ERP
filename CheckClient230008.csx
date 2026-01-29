#!/usr/bin/env dotnet-script
#r "src/QIMy.Infrastructure/bin/Debug/net8.0/QIMy.Infrastructure.dll"
#r "src/QIMy.Domain/bin/Debug/net8.0/QIMy.Domain.dll"
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.SqlServer, 8.0.0"

using QIMy.Infrastructure.Data;
using QIMy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var cs = Environment.GetEnvironmentVariable("QIMy_Azure_ConnectionString") ??
         "Server=tcp:qimy-accounting.database.windows.net,1433;Database=QImyDB;User ID=qimyadmin;Password=P@ssw0rd2024!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(cs)
    .Options;

using var context = new ApplicationDbContext(options);

Console.WriteLine("\n🔍 Checking client with code 230008:\n");

var clients = await context.Clients
    .Include(c => c.Business)
    .Where(c => c.ClientCode == 230008 && !c.IsDeleted)
    .OrderBy(c => c.BusinessId)
    .ToListAsync();

Console.WriteLine($"Found {clients.Count} clients with code 230008\n");

foreach (var client in clients)
{
    Console.WriteLine($"╔══════════════════════════════════════");
    Console.WriteLine($"║ ClientId: {client.Id}");
    Console.WriteLine($"║ BusinessId: {client.BusinessId}");
    Console.WriteLine($"║ Business: {client.Business?.CompanyName ?? "N/A"}");
    Console.WriteLine($"║ ClientCode: {client.ClientCode}");
    Console.WriteLine($"║ CompanyName: {client.CompanyName}");
    Console.WriteLine($"║ VAT: {client.VatNumber}");
    Console.WriteLine($"║ Created: {client.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"╚══════════════════════════════════════\n");
}

if (clients.Count > 1)
{
    Console.WriteLine($"⚠️  ПРОБЛЕМА: Клиент существует в {clients.Count} бизнесах!");
    Console.WriteLine("   При импорте BusinessId не был правильно установлен.");

    Console.WriteLine("\n🔧 Businesses in system:");
    var businesses = await context.Businesses.ToListAsync();
    foreach (var b in businesses)
    {
        var clientCount = clients.Count(c => c.BusinessId == b.Id);
        Console.WriteLine($"   - BusinessId={b.Id}: {b.CompanyName} ({clientCount} copies)");
    }
}
else if (clients.Count == 1)
{
    Console.WriteLine($"✅ OK: Клиент существует только в одном бизнесе (BusinessId={clients[0].BusinessId})");
}
else
{
    Console.WriteLine("❌ Клиент с кодом 230008 не найден.");
}

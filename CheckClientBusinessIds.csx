#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.SqlServer, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// Connection string
var connectionString = "Server=qimy-sql-server.database.windows.net;Database=QImyDb;User Id=adminuser;Password=Qz90op-=;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

var optionsBuilder = new DbContextOptionsBuilder();
optionsBuilder.UseSqlServer(connectionString);

using var context = new CheckClientBusinessIdsContext(optionsBuilder.Options);

Console.WriteLine("=== CHECKING CLIENT BUSINESS IDS ===\n");

// Get all clients
var clients = await context.Database.SqlQueryRaw<ClientInfo>(@"
    SELECT 
        Id,
        ClientCode,
        CompanyName,
        BusinessId
    FROM Clients
    ORDER BY ClientCode
").ToListAsync();

Console.WriteLine($"Total clients: {clients.Count}\n");

var clientsWithBusiness = clients.Where(c => c.BusinessId != null).ToList();
var clientsWithoutBusiness = clients.Where(c => c.BusinessId == null).ToList();

Console.WriteLine($"✅ Clients WITH BusinessId: {clientsWithBusiness.Count}");
foreach (var client in clientsWithBusiness)
{
    Console.WriteLine($"  - {client.ClientCode} | {client.CompanyName} | BusinessId={client.BusinessId}");
}

Console.WriteLine($"\n❌ Clients WITHOUT BusinessId: {clientsWithoutBusiness.Count}");
foreach (var client in clientsWithoutBusiness)
{
    Console.WriteLine($"  - {client.ClientCode} | {client.CompanyName} | BusinessId=NULL");
}

// Get all businesses
var businesses = await context.Database.SqlQueryRaw<BusinessInfo>(@"
    SELECT 
        Id,
        CompanyName
    FROM Businesses
").ToListAsync();

Console.WriteLine($"\n=== BUSINESSES ===");
foreach (var biz in businesses)
{
    Console.WriteLine($"ID: {biz.Id} | Name: {biz.CompanyName}");
}

Console.WriteLine("\n=== RECOMMENDATION ===");
if (clientsWithoutBusiness.Any())
{
    Console.WriteLine($"⚠️  Found {clientsWithoutBusiness.Count} clients without BusinessId.");
    Console.WriteLine("These clients need to be assigned to a business.");
    Console.WriteLine("\nTo fix this, run SQL:");
    Console.WriteLine($"UPDATE Clients SET BusinessId = <business_id> WHERE BusinessId IS NULL;");
}
else
{
    Console.WriteLine("✅ All clients have BusinessId assigned.");
}

public class CheckClientBusinessIdsContext : DbContext
{
    public CheckClientBusinessIdsContext(DbContextOptions options) : base(options) { }
}

public class ClientInfo
{
    public int Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public int? BusinessId { get; set; }
}

public class BusinessInfo
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

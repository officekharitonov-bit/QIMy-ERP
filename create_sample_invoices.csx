#!/usr/bin/env dotnet-script
#r "c:\Projects\QIMy\src\QIMy.Core\bin\Debug\net8.0\QIMy.Core.dll"
#r "c:\Projects\QIMy\src\QIMy.Infrastructure\bin\Debug\net8.0\QIMy.Infrastructure.dll"
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Design, 8.0.0"

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

var connectionString = "Data Source=c:\\Projects\\QIMy\\src\\QIMy.Web\\QImyDb.db";
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlite(connectionString);

using (var context = new ApplicationDbContext(optionsBuilder.Options))
{
    Console.WriteLine("=== Creating Test Invoices ===\n");
    
    var business = context.Businesses.FirstOrDefault();
    if (business == null) 
    {
        Console.WriteLine("Error: No business found!");
        return;
    }
    
    var clients = context.Clients.Where(c => !c.IsDeleted).ToList();
    var taxRate = context.TaxRates.FirstOrDefault(tr => tr.Rate == 20);
    
    if (clients.Count < 3)
    {
        Console.WriteLine($"Warning: Only {clients.Count} clients found, need 3 for demo");
    }
    
    // Create 3 sample invoices
    var invoices = new List<Invoice>();
    
    for (int i = 0; i < Math.Min(3, clients.Count); i++)
    {
        var client = clients[i];
        var invoiceNumber = $"2026{(i+1):D3}"; // 2026001, 2026002, 2026003
        
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = new DateTime(2026, 1, 15 + i),
            DueDate = new DateTime(2026, 2, 15 + i),
            BusinessId = business.Id,
            ClientId = client.Id,
            Currency = "EUR",
            Status = InvoiceStatus.Draft,
            // Invoice type determination based on client location
            InvoiceType = client.Country == "Austria" ? InvoiceType.Domestic : InvoiceType.IntraEU,
            TotalAmount = (i + 1) * 60m,
            TaxAmount = (i + 1) * 12m,
            GrossAmount = (i + 1) * 72m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Notes = $"Sample invoice for {client.CompanyName}"
        };
        
        invoices.Add(invoice);
        
        Console.WriteLine($"Created: Invoice {invoiceNumber} for {client.CompanyName} ({client.Country})");
    }
    
    context.Invoices.AddRange(invoices);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"\n✅ Successfully created {invoices.Count} invoices");
}

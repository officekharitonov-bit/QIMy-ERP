#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// Load DbContext
#load "src/QIMy.Infrastructure/Data/ApplicationDbContext.cs"
#load "src/QIMy.Core/Entities/*.cs"

var connectionString = "Data Source=QIMy.db";
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlite(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

Console.WriteLine("🔍 Поиск товаров с повреждённой кодировкой...");

// Find products with corrupted encoding (contains □ or null bytes)
var corruptedProducts = context.Products
    .Where(p => p.Name.Contains("□") || p.SKU.Contains("□"))
    .ToList();

Console.WriteLine($"Найдено товаров с 'кубиками': {corruptedProducts.Count}");

if (corruptedProducts.Any())
{
    Console.Write("❓ Удалить все? (y/n): ");
    var confirm = Console.ReadLine()?.Trim().ToLower();

    if (confirm == "y" || confirm == "yes")
    {
        context.Products.RemoveRange(corruptedProducts);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ Удалено {corruptedProducts.Count} товаров");
        Console.WriteLine("✅ Теперь можете переимпортировать CSV с правильной кодировкой!");
    }
    else
    {
        Console.WriteLine("❌ Операция отменена");
    }
}
else
{
    Console.WriteLine("✅ Товары с повреждённой кодировкой не найдены");
}

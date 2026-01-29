// Test AI Encoding Detection on BKHA CSV files
#r "nuget: Microsoft.Extensions.DependencyInjection, 8.0.0"

using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

// Add reference to compiled DLLs
#r "C:\Projects\QIMy\src\QIMy.Core\bin\Debug\net8.0\QIMy.Core.dll"
#r "C:\Projects\QIMy\src\QIMy.AI\bin\Debug\net8.0\QIMy.AI.dll"

using QIMy.AI.Services;

Console.WriteLine("=== AI Encoding Detection Test ===\n");

// Setup DI
var services = new ServiceCollection();
services.AddScoped<IAiEncodingDetectionService, AiEncodingDetectionService>();
var serviceProvider = services.BuildServiceProvider();

var encodingService = serviceProvider.GetRequiredService<IAiEncodingDetectionService>();

// Test files
var testFiles = new[]
{
    @"C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Clients_BKHA_Import.csv",
    @"C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Suppliers_BKHA_Import.csv",
    @"C:\Projects\QIMy\tabellen\BKHA GmbH\BH\Sachkonten 2025 BKHA GmbH - 26.01.2025.csv"
};

foreach (var filePath in testFiles)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"❌ File not found: {filePath}\n");
        continue;
    }

    Console.WriteLine($"📄 Testing: {Path.GetFileName(filePath)}");
    Console.WriteLine($"   Path: {filePath}");

    try
    {
        using var stream = File.OpenRead(filePath);
        var result = await encodingService.DetectEncodingAsync(stream);

        Console.WriteLine($"✅ Detected Encoding: {result.Encoding.EncodingName} ({result.Encoding.CodePage})");
        Console.WriteLine($"   Confidence: {result.Confidence:P1}");
        Console.WriteLine($"   Method: {result.DetectionMethod}");

        if (!string.IsNullOrEmpty(result.Details))
        {
            Console.WriteLine($"   Details: {result.Details}");
        }

        if (result.Alternatives.Any())
        {
            Console.WriteLine($"   Alternatives:");
            foreach (var alt in result.Alternatives.Take(3))
            {
                Console.WriteLine($"     - {alt.Encoding.EncodingName} ({alt.Confidence:P1}): {alt.Reason}");
            }
        }

        // Test reading with detected encoding
        stream.Position = 0;
        using var reader = new StreamReader(stream, result.Encoding);
        var firstLine = reader.ReadLine();
        Console.WriteLine($"   First line preview: {firstLine?.Substring(0, Math.Min(100, firstLine?.Length ?? 0))}...");

        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}\n");
    }
}

Console.WriteLine("=== Test Complete ===");

#r "nuget: ClosedXML, 0.102.2"

using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

var excelPath = @"c:\Temp\Personen_index_temp.xlsx";

Console.WriteLine($"File exists: {File.Exists(excelPath)}");

if (!File.Exists(excelPath))
    return;

try {
    using var workbook = new XLWorkbook(excelPath);
    Console.WriteLine($"Workbook opened, sheets: {workbook.Worksheets.Count}");
    
    // List all sheet names
    foreach (var ws in workbook.Worksheets)
    {
        Console.WriteLine($"  Sheet {ws.Position}: {ws.Name}");
    }
    
    // Check Sheet 6 structure (Länder)
    Console.WriteLine("\n=== Sheet 6 Structure ===");
    var ws6 = workbook.Worksheet(6);
    var rows6 = ws6.RowsUsed().Take(3).ToList();
    foreach (var row in rows6)
    {
        var cells = row.CellsUsed().Take(5);
        var values = string.Join(" | ", cells.Select(c => c.GetValue<object>()?.ToString() ?? "[empty]"));
        Console.WriteLine($"Row {row.RowNumber}: {values}");
    }
    
    // Check Sheet 2 structure (EU-RATE)
    Console.WriteLine("\n=== Sheet 2 Structure ===");
    var ws2 = workbook.Worksheet(2);
    var rows2 = ws2.RowsUsed().Take(3).ToList();
    foreach (var row in rows2)
    {
        var cells = row.CellsUsed().Take(10);
        var values = string.Join(" | ", cells.Select(c => c.GetValue<object>()?.ToString() ?? "[empty]"));
        Console.WriteLine($"Row {row.RowNumber}: {values}");
    }
    
    Console.WriteLine("\nSuccess!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}

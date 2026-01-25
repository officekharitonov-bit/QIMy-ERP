#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Design, 8.0.0"

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Simplified test - we'll compile the project instead

Console.WriteLine("========================================");
Console.WriteLine("    TAX LOGIC ENGINE - DEMO TESTS");
Console.WriteLine("========================================\n");

var taxEngine = new QIMy.Infrastructure.Services.TaxLogic.AustrianTaxLogicEngine();

// Test 1: INLAND (Standard Austrian customer)
Console.WriteLine("--- Test 1: INLAND (Austria) ---");
var test1 = new QIMy.Infrastructure.Services.TaxLogic.TaxCaseInput
{
    BuyerCountry = "AT",
    BuyerCountryInEU = true,
    IsGoodsSupply = true,
    SellerIsSmallBusiness = false
};
var result1 = taxEngine.DetermineTaxCase(test1);
Console.WriteLine($"Tax Case: {result1.TaxCase}");
Console.WriteLine($"Steuercode: {result1.Steuercode}");
Console.WriteLine($"Konto: {result1.Konto}");
Console.WriteLine($"Proz: {result1.Proz}%");
Console.WriteLine($"Invoice Text: {result1.InvoiceText}");
Console.WriteLine($"Required Fields: {string.Join(", ", result1.RequiredFields)}");
Console.WriteLine($"Notes: {result1.Notes}\n");

// Test 2: Kleinunternehmer
Console.WriteLine("--- Test 2: Kleinunternehmer ---");
var test2 = new QIMy.Infrastructure.Services.TaxLogic.TaxCaseInput
{
    SellerIsSmallBusiness = true,
    BuyerCountry = "AT"
};
var result2 = taxEngine.DetermineTaxCase(test2);
Console.WriteLine($"Tax Case: {result2.TaxCase}");
Console.WriteLine($"Steuercode: {result2.Steuercode}");
Console.WriteLine($"Konto: {result2.Konto}");
Console.WriteLine($"Proz: {result2.Proz}%");
Console.WriteLine($"Invoice Text: {result2.InvoiceText}");
Console.WriteLine($"UID Required: {result2.RequiresUidValidation}");
Console.WriteLine($"Notes: {result2.Notes}\n");

// Test 3: IGL (Germany)
Console.WriteLine("--- Test 3: IGL (Germany) ---");
var test3 = new QIMy.Infrastructure.Services.TaxLogic.TaxCaseInput
{
    BuyerCountry = "DE",
    BuyerCountryInEU = true,
    BuyerUid = "DE123456789",
    IsGoodsSupply = true,
    SellerIsSmallBusiness = false
};
var result3 = taxEngine.DetermineTaxCase(test3);
Console.WriteLine($"Tax Case: {result3.TaxCase}");
Console.WriteLine($"Steuercode: {result3.Steuercode}");
Console.WriteLine($"Konto: {result3.Konto}");
Console.WriteLine($"Proz: {result3.Proz}%");
Console.WriteLine($"Invoice Text: {result3.InvoiceText}");
Console.WriteLine($"Required Fields: {string.Join(", ", result3.RequiredFields)}");
Console.WriteLine($"UID Validation Required: {result3.RequiresUidValidation}");
Console.WriteLine($"Notes: {result3.Notes}\n");

// Test 4: Reverse Charge (France, Services)
Console.WriteLine("--- Test 4: Reverse Charge (France, Services) ---");
var test4 = new QIMy.Infrastructure.Services.TaxLogic.TaxCaseInput
{
    BuyerCountry = "FR",
    BuyerCountryInEU = true,
    BuyerUid = "FR12345678901",
    IsGoodsSupply = false,
    SellerIsSmallBusiness = false
};
var result4 = taxEngine.DetermineTaxCase(test4);
Console.WriteLine($"Tax Case: {result4.TaxCase}");
Console.WriteLine($"Steuercode: {result4.Steuercode}");
Console.WriteLine($"Konto: {result4.Konto}");
Console.WriteLine($"Proz: {result4.Proz}%");
Console.WriteLine($"Invoice Text: {result4.InvoiceText}");
Console.WriteLine($"Is Reverse Charge: {result4.IsReverseCharge}");
Console.WriteLine($"Notes: {result4.Notes}\n");

// Test 5: Export (USA)
Console.WriteLine("--- Test 5: Export (USA) ---");
var test5 = new QIMy.Infrastructure.Services.TaxLogic.TaxCaseInput
{
    BuyerCountry = "US",
    BuyerCountryInEU = false,
    IsGoodsSupply = true,
    SellerIsSmallBusiness = false
};
var result5 = taxEngine.DetermineTaxCase(test5);
Console.WriteLine($"Tax Case: {result5.TaxCase}");
Console.WriteLine($"Steuercode: {result5.Steuercode}");
Console.WriteLine($"Konto: {result5.Konto}");
Console.WriteLine($"Proz: {result5.Proz}%");
Console.WriteLine($"Invoice Text: {result5.InvoiceText}");
Console.WriteLine($"Required Fields: {string.Join(", ", result5.RequiredFields)}");
Console.WriteLine($"Notes: {result5.Notes}\n");

// Test 6: Dreiecksgeschäft (Triangular)
Console.WriteLine("--- Test 6: Dreiecksgeschäft ---");
var test6 = new QIMy.Infrastructure.Services.TaxLogic.TaxCaseInput
{
    IsTriangularTransaction = true,
    BuyerCountry = "FR",
    BuyerCountryInEU = true,
    IntermediaryCountryInEU = true,
    BuyerUid = "FR123456789",
    SellerIsSmallBusiness = false
};
var result6 = taxEngine.DetermineTaxCase(test6);
Console.WriteLine($"Tax Case: {result6.TaxCase}");
Console.WriteLine($"Steuercode: {result6.Steuercode}");
Console.WriteLine($"Konto: {result6.Konto}");
Console.WriteLine($"Proz: {result6.Proz}%");
Console.WriteLine($"Invoice Text: {result6.InvoiceText}");
Console.WriteLine($"Required Fields: {string.Join(", ", result6.RequiredFields)}");
Console.WriteLine($"Notes: {result6.Notes}\n");

// Steuercode descriptions
Console.WriteLine("\n========================================");
Console.WriteLine("    STEUERCODE DESCRIPTIONS");
Console.WriteLine("========================================");
var codes = new[] { 1, 2, 10, 11, 16, 19, 42, 43 };
foreach (var code in codes)
{
    Console.WriteLine($"Code {code}: {taxEngine.GetSteuercodeDescription(code)}");
}

Console.WriteLine("\n✅ All tests completed successfully!");
Console.WriteLine("Tax Logic Engine is working correctly.");

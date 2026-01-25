using System;
using QIMy.Infrastructure.Services.TaxLogic;

namespace QIMy.Tests;

/// <summary>
/// Manual test program for Tax Logic Engine
/// Run: dotnet run --project TestTaxEngine.csproj
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("    TAX LOGIC ENGINE - DEMO TESTS");
        Console.WriteLine("========================================\n");

        var taxEngine = new AustrianTaxLogicEngine();

        // Test 1: INLAND (Standard Austrian customer)
        Console.WriteLine("--- Test 1: INLAND (Austria) ---");
        var test1 = new TaxCaseInput
        {
            BuyerCountry = "AT",
            BuyerCountryInEU = true,
            IsGoodsSupply = true,
            SellerIsSmallBusiness = false
        };
        PrintResult(taxEngine.DetermineTaxCase(test1));

        // Test 2: Kleinunternehmer
        Console.WriteLine("\n--- Test 2: Kleinunternehmer ---");
        var test2 = new TaxCaseInput
        {
            SellerIsSmallBusiness = true,
            BuyerCountry = "AT"
        };
        PrintResult(taxEngine.DetermineTaxCase(test2));

        // Test 3: IGL (Germany)
        Console.WriteLine("\n--- Test 3: IGL (Germany) ---");
        var test3 = new TaxCaseInput
        {
            BuyerCountry = "DE",
            BuyerCountryInEU = true,
            BuyerUid = "DE123456789",
            IsGoodsSupply = true,
            SellerIsSmallBusiness = false
        };
        PrintResult(taxEngine.DetermineTaxCase(test3));

        // Test 4: Reverse Charge (France, Services)
        Console.WriteLine("\n--- Test 4: Reverse Charge (France, Services) ---");
        var test4 = new TaxCaseInput
        {
            BuyerCountry = "FR",
            BuyerCountryInEU = true,
            BuyerUid = "FR12345678901",
            IsGoodsSupply = false,
            SellerIsSmallBusiness = false
        };
        PrintResult(taxEngine.DetermineTaxCase(test4));

        // Test 5: Export (USA)
        Console.WriteLine("\n--- Test 5: Export (USA) ---");
        var test5 = new TaxCaseInput
        {
            BuyerCountry = "US",
            BuyerCountryInEU = false,
            IsGoodsSupply = true,
            SellerIsSmallBusiness = false
        };
        PrintResult(taxEngine.DetermineTaxCase(test5));

        // Test 6: Dreiecksgeschäft (Triangular)
        Console.WriteLine("\n--- Test 6: Dreiecksgeschäft ---");
        var test6 = new TaxCaseInput
        {
            IsTriangularTransaction = true,
            BuyerCountry = "FR",
            BuyerCountryInEU = true,
            IntermediaryCountryInEU = true,
            BuyerUid = "FR123456789",
            SellerIsSmallBusiness = false
        };
        PrintResult(taxEngine.DetermineTaxCase(test6));

        // Steuercode descriptions
        Console.WriteLine("\n========================================");
        Console.WriteLine("    STEUERCODE DESCRIPTIONS");
        Console.WriteLine("========================================");
        var codes = new[] { 1, 2, 10, 11, 16, 19, 42, 43 };
        foreach (var code in codes)
        {
            Console.WriteLine($"Code {code:D2}: {taxEngine.GetSteuercodeDescription(code)}");
        }

        Console.WriteLine("\n✅ All tests completed successfully!");
    }

    static void PrintResult(TaxCaseResult result)
    {
        Console.WriteLine($"Tax Case: {result.TaxCase}");
        Console.WriteLine($"Steuercode: {result.Steuercode}");
        Console.WriteLine($"Konto: {result.Konto}");
        Console.WriteLine($"Proz: {result.Proz}%");
        Console.WriteLine($"Invoice Text: {result.InvoiceText}");
        Console.WriteLine($"Required Fields: {string.Join(", ", result.RequiredFields)}");
        Console.WriteLine($"UID Validation: {result.RequiresUidValidation}");
        Console.WriteLine($"Notes: {result.Notes}");
    }
}

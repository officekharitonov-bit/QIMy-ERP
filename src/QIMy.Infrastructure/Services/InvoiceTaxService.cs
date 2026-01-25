using QIMy.Core.Entities;
using QIMy.Infrastructure.Services.TaxLogic;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Service for applying Austrian tax logic to invoices
/// Integrates TaxLogicEngine with Invoice entity
/// </summary>
public class InvoiceTaxService
{
    private readonly AustrianTaxLogicEngine _taxEngine;

    public InvoiceTaxService()
    {
        _taxEngine = new AustrianTaxLogicEngine();
    }

    /// <summary>
    /// Apply tax logic to invoice based on client and company data
    /// </summary>
    public void ApplyTaxLogic(Invoice invoice, Client client, bool sellerIsSmallBusiness, bool isGoodsSupply = true)
    {
        // Prepare input for tax engine
        var input = new TaxCaseInput
        {
            SellerIsSmallBusiness = sellerIsSmallBusiness,
            BuyerCountry = client.Country ?? "AT",
            BuyerCountryInEU = IsEUCountry(client.Country),
            BuyerUid = client.VatNumber,
            IsGoodsSupply = isGoodsSupply,
            ReducedVatRate = false // TODO: make configurable
        };

        // Determine tax case
        var taxResult = _taxEngine.DetermineTaxCase(input);

        // Apply to invoice
        invoice.InvoiceType = taxResult.InvoiceType;
        invoice.IsReverseCharge = taxResult.IsReverseCharge;
        invoice.IsSmallBusinessExemption = taxResult.IsSmallBusinessExemption;
        invoice.IsTaxFreeExport = taxResult.IsTaxFreeExport;
        invoice.IsIntraEUSale = taxResult.IsIntraEUSale;

        // NEW: Apply FIBU posting parameters
        invoice.Steuercode = taxResult.Steuercode;
        invoice.Konto = taxResult.Konto.ToString();
        invoice.Proz = taxResult.Proz;

        // Calculate tax amounts
        invoice.TaxAmount = CalculateTax(invoice.SubTotal, taxResult.VatRate);
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;
    }

    /// <summary>
    /// Calculate tax amount based on subtotal and VAT rate
    /// </summary>
    private decimal CalculateTax(decimal subTotal, decimal vatRate)
    {
        return Math.Round(subTotal * (vatRate / 100), 2);
    }

    /// <summary>
    /// Get invoice text for PDF based on tax case result
    /// </summary>
    public string GetInvoiceText(Invoice invoice)
    {
        if (!invoice.Steuercode.HasValue)
        {
            return "Umsatzsteuer 20%"; // Default
        }

        var dummyInput = new TaxCaseInput { BuyerCountry = "AT" };
        var result = _taxEngine.DetermineTaxCase(dummyInput);

        // Map Steuercode back to text
        return invoice.Steuercode switch
        {
            1 => "Umsatzsteuer 20%",
            2 => "Umsatzsteuer 10%",
            10 => "Steuerfreie Ausfuhrlieferung gem. § 6 Abs. 1 Z 1 UStG",
            11 => "Steuerfreie innergemeinschaftliche Lieferung gem. Art. 6 Abs. 1 UStG",
            16 => "Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG",
            19 => "Steuerschuldner ist der Rechnungsempfänger (Reverse Charge gem. § 19 UStG)",
            _ => $"Steuercode {invoice.Steuercode}"
        };
    }

    /// <summary>
    /// Validate invoice before saving - check required fields
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateInvoice(Invoice invoice, Client client)
    {
        var errors = new List<string>();

        // Determine tax case
        var input = new TaxCaseInput
        {
            BuyerCountry = client.Country ?? "AT",
            BuyerCountryInEU = IsEUCountry(client.Country),
            BuyerUid = client.VatNumber
        };

        var taxResult = _taxEngine.DetermineTaxCase(input);

        // Check required fields based on tax case
        if (taxResult.RequiresUidValidation)
        {
            if (string.IsNullOrEmpty(client.VatNumber))
            {
                errors.Add($"UID Käufer ist erforderlich für {taxResult.TaxCase}");
            }
            else if (!IsValidUidFormat(client.VatNumber))
            {
                errors.Add("UID Käufer hat ungültiges Format (z.B. ATU12345678)");
            }
        }

        // Check tax case specific requirements
        switch (taxResult.TaxCase)
        {
            case TaxCase.InnergemeinschaftlicheLieferung:
                if (string.IsNullOrEmpty(client.VatNumber))
                    errors.Add("UID Käufer ist Pflicht für IGL");
                break;

            case TaxCase.ReverseCharge:
                if (string.IsNullOrEmpty(client.VatNumber))
                    errors.Add("UID Käufer ist Pflicht für Reverse Charge");
                break;

            case TaxCase.Export:
                // Could check for customs number here
                break;
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Check if country is in EU
    /// </summary>
    private bool IsEUCountry(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode)) return false;

        var euCountries = new[]
        {
            "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
            "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
            "PL", "PT", "RO", "SK", "SI", "ES", "SE"
        };

        return euCountries.Contains(countryCode.ToUpper());
    }

    /// <summary>
    /// Basic UID format validation
    /// </summary>
    private bool IsValidUidFormat(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return false;

        // Basic check: Should start with 2-letter country code + "U" + numbers
        // Example: ATU12345678, DE123456789
        return uid.Length >= 8 && 
               char.IsLetter(uid[0]) && 
               char.IsLetter(uid[1]);
    }

    /// <summary>
    /// Get all EU country codes
    /// </summary>
    public static string[] GetEUCountries()
    {
        return new[]
        {
            "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
            "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
            "PL", "PT", "RO", "SK", "SI", "ES", "SE"
        };
    }
}

using QIMy.Core.Entities;

namespace QIMy.Infrastructure.Services.TaxLogic;

/// <summary>
/// Tax Logic Engine for Austrian VAT (UStG) compliance
/// Автоматически определяет налоговый случай и генерирует параметры для проводки
/// Based on BMD NTCS logic and Austrian UStG law
/// </summary>
public class AustrianTaxLogicEngine
{
    /// <summary>
    /// Analyze transaction and determine tax case with all required parameters
    /// </summary>
    public TaxCaseResult DetermineTaxCase(TaxCaseInput input)
    {
        // Case 1: Kleinunternehmer (Small Business)
        if (input.SellerIsSmallBusiness)
        {
            return new TaxCaseResult
            {
                TaxCase = TaxCase.Kleinunternehmer,
                InvoiceType = InvoiceType.SmallBusinessExemption,
                Steuercode = 16,
                Proz = 0m,
                Konto = 4062,
                InvoiceText = "Kleinunternehmer gem. § 6 Abs. 1 Z 27 UStG",
                VatRate = 0m,
                RequiredFields = new[] { "Firmenname", "Adresse" },
                Notes = "НДС не выделяется. UID не требуется и не может быть указан.",
                IsReverseCharge = false,
                IsTaxFree = true,
                IsSmallBusinessExemption = true,
                RequiresUidValidation = false
            };
        }

        // Case 2: Innergemeinschaftliche Lieferung (IGL - Intra-EU Supply)
        if (input.BuyerCountryInEU && input.BuyerCountry != "AT" &&
            input.IsGoodsSupply && !string.IsNullOrEmpty(input.BuyerUid))
        {
            return new TaxCaseResult
            {
                TaxCase = TaxCase.InnergemeinschaftlicheLieferung,
                InvoiceType = InvoiceType.IntraEUSale,
                Steuercode = 11, // Umsätze Art. 6 Abs. 1
                Proz = 0m,
                Konto = 4000, // Standard Erlöskonto
                InvoiceText = "Steuerfreie innergemeinschaftliche Lieferung gem. Art. 6 Abs. 1 UStG",
                VatRate = 0m,
                RequiredFields = new[] { "UID Verkäufer (ATU)", "UID Käufer", "Lieferadresse EU" },
                Notes = "UID обоих сторон обязательны. Товар физически перемещается в другую страну ЕС.",
                IsReverseCharge = false,
                IsTaxFree = true,
                IsIntraEUSale = true,
                RequiresUidValidation = true
            };
        }

        // Case 3: Reverse Charge (Services within EU / B2B)
        if (input.BuyerCountryInEU && input.BuyerCountry != "AT" &&
            input.IsServiceSupply && !string.IsNullOrEmpty(input.BuyerUid))
        {
            return new TaxCaseResult
            {
                TaxCase = TaxCase.ReverseCharge,
                InvoiceType = InvoiceType.ReverseCharge,
                Steuercode = 19, // Umsätze § 19
                Proz = 0m,
                Konto = 4000,
                InvoiceText = "Steuerschuldner ist der Rechnungsempfänger (Reverse Charge gem. § 19 UStG)",
                VatRate = 0m,
                RequiredFields = new[] { "UID Verkäufer", "UID Käufer" },
                Notes = "Услуги B2B внутри ЕС. Налоговая обязанность на покупателе.",
                IsReverseCharge = true,
                IsTaxFree = false,
                RequiresUidValidation = true
            };
        }

        // Case 4: Export (Third Countries - Outside EU)
        if (!input.BuyerCountryInEU && input.BuyerCountry != "AT")
        {
            return new TaxCaseResult
            {
                TaxCase = TaxCase.Export,
                InvoiceType = InvoiceType.Export,
                Steuercode = 10, // Ausfuhrlieferung § 6 Abs. 1 Z 1
                Proz = 0m,
                Konto = 4000,
                InvoiceText = "Steuerfreie Ausfuhrlieferung gem. § 6 Abs. 1 Z 1 UStG",
                VatRate = 0m,
                RequiredFields = new[] { "Firmenname", "Lieferadresse außerhalb EU", "Zollnummer" },
                Notes = "Экспорт за пределы ЕС. Требуется таможенное подтверждение.",
                IsReverseCharge = false,
                IsTaxFree = true,
                IsTaxFreeExport = true,
                RequiresUidValidation = false
            };
        }

        // Case 5: Dreiecksgeschäft (Triangular Transaction)
        if (input.IsTriangularTransaction &&
            input.BuyerCountryInEU && input.IntermediaryCountryInEU)
        {
            return new TaxCaseResult
            {
                TaxCase = TaxCase.Dreiecksgeschaeft,
                InvoiceType = InvoiceType.TriangularTransaction,
                Steuercode = 11, // Same as IGL
                Proz = 0m,
                Konto = 4000,
                InvoiceText = "Innergemeinschaftliches Dreiecksgeschäft gem. Art. 25 UStG. Die Steuerschuld geht auf den Empfänger über.",
                VatRate = 0m,
                RequiredFields = new[] { "UID всех трех сторон", "Lieferkette документация" },
                Notes = "Трёхсторонняя сделка между тремя странами ЕС. Особый режим.",
                IsReverseCharge = true,
                IsTaxFree = false,
                RequiresUidValidation = true
            };
        }

        // Case 6: INLAND (Domestic - Standard AT supply)
        if (input.BuyerCountry == "AT" || string.IsNullOrEmpty(input.BuyerCountry))
        {
            var vatRate = input.ReducedVatRate ? 10m : 20m; // Standard 20%, Reduced 10%
            var steuercode = input.ReducedVatRate ? 2 : 1; // 1=VSt, 2=Vorsteuer

            return new TaxCaseResult
            {
                TaxCase = TaxCase.Inland,
                InvoiceType = InvoiceType.Domestic,
                Steuercode = steuercode,
                Proz = vatRate,
                Konto = 4000,
                InvoiceText = $"Umsatzsteuer {vatRate}%",
                VatRate = vatRate,
                RequiredFields = new[] { "Firmenname", "Adresse", "UID (optional für B2B)" },
                Notes = "Стандартная поставка в Австрии. НДС выделяется отдельно.",
                IsReverseCharge = false,
                IsTaxFree = false,
                RequiresUidValidation = false
            };
        }

        // Default: INLAND
        return new TaxCaseResult
        {
            TaxCase = TaxCase.Inland,
            InvoiceType = InvoiceType.Domestic,
            Steuercode = 1,
            Proz = 20m,
            Konto = 4000,
            InvoiceText = "Umsatzsteuer 20%",
            VatRate = 20m,
            RequiredFields = new[] { "Firmenname", "Adresse" },
            Notes = "Default case - Inland standard VAT",
            IsReverseCharge = false,
            IsTaxFree = false,
            RequiresUidValidation = false
        };
    }

    /// <summary>
    /// Get Steuercode description from Austrian tax code table
    /// </summary>
    public string GetSteuercodeDescription(int steuercode) => steuercode switch
    {
        1 => "Umsatzsteuer",
        2 => "Vorsteuer",
        3 => "VSt Art 12/23 (7m Abs. 4 und 5)",
        4 => "VSt f. igl. neuer Fahrzeuge gem. Art. 2",
        5 => "Ausfuhrlieferungen",
        6 => "Übriges Dreiecksgeschäfte",
        7 => "ig Lieferung",
        8 => "Aufw. ig Erwerb o. VSt-Abzug",
        9 => "Aufw. ig Erwerb m. VSt-Abzug",
        10 => "Erwerbe gem. Art. 3/8",
        11 => "Erwerbe gem. Art. 3/8, Art. 25/2",
        12 => "Eigenverbrauch",
        13 => "Lohnveredelung §6/1 Z 1 iVm §8",
        14 => "Personenbeförderung §6/1 Z 2-6 sowie §23/5",
        15 => "Grundstücksumsätze §6/1 Z 9",
        16 => "Kleinunternehmer §6/1 Z 27",
        17 => "Übrige Umsätze o. VSt-Abzug §6/1 Z_",
        18 => "Aufw. §19/1 Reverse Charge o. VSt-Abzug",
        19 => "Aufw. §19/1 Reverse Charge m. VSt-Abzug",
        20 => "Umsätze grenzüb DL §6 Ausfuhr (nicht ZM-pflichtig)",
        21 => "Umsätze §19/1b",
        22 => "Aufw. §19/1b o. VSt-Abzug",
        23 => "Aufw. §19/1b m. VSt-Abzug",
        24 => "Umsätze §19/1c",
        25 => "Aufw. §19/1c o. VSt-Abzug",
        26 => "Aufw. §19/1c m. VSt-Abzug",
        27 => "Umsätze §19/1a Bauleistungen",
        42 => "VSt nicht abzugsfähig",
        43 => "Steuerschuld gem. §11/12 und 14, §16/2",
        44 => "VSt in KZ 066 §19/1 betreffend KFZ",
        _ => $"Unknown Steuercode {steuercode}"
    };
}

/// <summary>
/// Input data for tax case determination
/// </summary>
public class TaxCaseInput
{
    /// <summary>Seller is small business (Kleinunternehmer)</summary>
    public bool SellerIsSmallBusiness { get; set; }

    /// <summary>Buyer country code (AT, DE, US, etc.)</summary>
    public string BuyerCountry { get; set; } = "AT";

    /// <summary>Is buyer country in EU?</summary>
    public bool BuyerCountryInEU { get; set; }

    /// <summary>Buyer VAT ID (UID)</summary>
    public string? BuyerUid { get; set; }

    /// <summary>Is this a supply of goods (true) or services (false)?</summary>
    public bool IsGoodsSupply { get; set; } = true;

    /// <summary>Is this a supply of services?</summary>
    public bool IsServiceSupply => !IsGoodsSupply;

    /// <summary>Is this a triangular transaction (Dreiecksgeschäft)?</summary>
    public bool IsTriangularTransaction { get; set; }

    /// <summary>Intermediary country in EU (for triangular)</summary>
    public bool IntermediaryCountryInEU { get; set; }

    /// <summary>Use reduced VAT rate (10% instead of 20%)?</summary>
    public bool ReducedVatRate { get; set; }
}

/// <summary>
/// Result of tax case determination with all required parameters
/// </summary>
public class TaxCaseResult
{
    /// <summary>Determined tax case</summary>
    public TaxCase TaxCase { get; set; }

    /// <summary>Invoice type for QIMy system</summary>
    public InvoiceType InvoiceType { get; set; }

    /// <summary>BMD Steuercode (налоговый код)</summary>
    public int Steuercode { get; set; }

    /// <summary>Prozentsatz (процентная ставка)</summary>
    public decimal Proz { get; set; }

    /// <summary>Erlöskonto (счет доходов)</summary>
    public int Konto { get; set; }

    /// <summary>Required text for invoice (mandatory by law)</summary>
    public string InvoiceText { get; set; } = string.Empty;

    /// <summary>VAT rate in percent</summary>
    public decimal VatRate { get; set; }

    /// <summary>Required fields for this tax case</summary>
    public string[] RequiredFields { get; set; } = Array.Empty<string>();

    /// <summary>Additional notes for bookkeeper/developer</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Flags for Invoice entity</summary>
    public bool IsReverseCharge { get; set; }
    public bool IsTaxFree { get; set; }
    public bool IsSmallBusinessExemption { get; set; }
    public bool IsTaxFreeExport { get; set; }
    public bool IsIntraEUSale { get; set; }

    /// <summary>Requires UID validation via VIES?</summary>
    public bool RequiresUidValidation { get; set; }
}

/// <summary>
/// Austrian tax cases (Steuerfälle)
/// </summary>
public enum TaxCase
{
    /// <summary>Standard domestic supply with 20% VAT</summary>
    Inland,

    /// <summary>Export to non-EU countries (0% VAT)</summary>
    Export,

    /// <summary>Intra-EU supply of goods (0% VAT, reverse charge)</summary>
    InnergemeinschaftlicheLieferung,

    /// <summary>Reverse charge for services within EU (0% VAT)</summary>
    ReverseCharge,

    /// <summary>Small business exemption (0% VAT)</summary>
    Kleinunternehmer,

    /// <summary>Triangular transaction between 3 EU countries</summary>
    Dreiecksgeschaeft
}

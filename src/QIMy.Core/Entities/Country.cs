namespace QIMy.Core.Entities;

/// <summary>
/// Country entity - master data for countries
/// Populated from "Länder" sheet of Personen Index
/// </summary>
public class Country : BaseEntity
{
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2): AT, DE, GB, etc.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Country name in German (e.g., "Österreich")
    /// </summary>
    public string NameGerman { get; set; } = string.Empty;
    
    /// <summary>
    /// Country name in English (e.g., "Austria")
    /// </summary>
    public string NameEnglish { get; set; } = string.Empty;
    
    /// <summary>
    /// Country number from Personen Index (Land-NR)
    /// </summary>
    public int CountryNumber { get; set; }
    
    /// <summary>
    /// Whether this is an EU member state
    /// </summary>
    public bool IsEuMember { get; set; } = false;
    
    /// <summary>
    /// Currency code (ISO 4217): EUR, GBP, USD, etc.
    /// </summary>
    public string CurrencyCode { get; set; } = "EUR";
    
    /// <summary>
    /// Navigation: EU-specific data if this is an EU country
    /// </summary>
    public EuCountryData? EuData { get; set; }
    
    /// <summary>
    /// Navigation: Tax rates for this country
    /// </summary>
    public ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();
}

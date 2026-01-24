namespace QIMy.Core.Entities;

/// <summary>
/// EU Country specific data - thresholds and VAT information
/// Populated from "EU-RATE" sheet of Personen Index
/// </summary>
public class EuCountryData : BaseEntity
{
    /// <summary>
    /// Foreign key to Country
    /// </summary>
    public int CountryId { get; set; }
    
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2): AT, DE, GB, etc.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Purchase threshold (Erwerbsschwelle) in local currency
    /// Example: Austria = 11,000 EUR
    /// </summary>
    public decimal PurchaseThreshold { get; set; }
    
    /// <summary>
    /// Supply threshold (Lieferschwelle) in local currency
    /// Example: Austria = 35,000 EUR
    /// </summary>
    public decimal SupplyThreshold { get; set; }
    
    /// <summary>
    /// Account number for this country (Kto-Nr)
    /// Example: Austria = 4000, Germany = 4001
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Country number from Excel (Land-NR)
    /// </summary>
    public int CountryNumber { get; set; }
    
    /// <summary>
    /// Additional notes about VAT/tax rules
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Date when this data was last verified
    /// </summary>
    public DateTime LastVerified { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation: Country
    /// </summary>
    public Country Country { get; set; } = null!;
}

namespace QIMy.Core.Entities;

/// <summary>
/// VAT/Tax rate entity - stores historical rates with effective dates
/// Updated automatically via Vatlayer API
/// </summary>
public class TaxRate : BaseEntity
{
    public int? BusinessId { get; set; }
    public Business? Business { get; set; }
    
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2): AT, DE, GB, etc.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Country name: Austria, Germany, United Kingdom, etc.
    /// </summary>
    public string CountryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name: "Standard VAT (AT)", "Reduced VAT 10% (AT)", etc.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// VAT rate percentage (e.g., 20.00 for 20%)
    /// </summary>
    public decimal Rate { get; set; }
    
    /// <summary>
    /// Type of rate: Standard, Reduced, SuperReduced, Parking
    /// </summary>
    public TaxRateType RateType { get; set; } = TaxRateType.Standard;
    
    /// <summary>
    /// Date from which this rate is effective
    /// </summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date until which this rate is effective (null = current)
    /// </summary>
    public DateTime? EffectiveUntil { get; set; }
    
    /// <summary>
    /// Whether this is the default rate for this business
    /// </summary>
    public bool IsDefault { get; set; } = false;
    
    /// <summary>
    /// Source of data: Manual, VatlayerAPI, Excel
    /// </summary>
    public string Source { get; set; } = "Manual";
    
    /// <summary>
    /// Additional notes or reason for rate change
    /// </summary>
    public string? Notes { get; set; }
}

public enum TaxRateType
{
    Standard = 1,
    Reduced = 2,
    SuperReduced = 3,
    Parking = 4,
    Zero = 5
}
